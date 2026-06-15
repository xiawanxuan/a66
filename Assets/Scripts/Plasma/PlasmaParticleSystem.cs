using UnityEngine;
using System.Collections.Generic;

public class PlasmaParticleSystem : MonoBehaviour
{
    public int maxParticles = 80000;
    public int emitRate = 2000;
    public float particleLifetime = 2f;
    public float electronMobility = 0.03f;
    public float ionMobility = 1.5e-4f;
    public float recombinationCoeff = 2.0e-13f;
    public float domainSize = 1f;
    public float ambientTemp = 300f;
    public float lorentzForceScale = 1e6f;

    private ParticleState[] particles;
    private int aliveCount;
    private int[] freeIndices;
    private int freeIndexHead;

    private Vector3[] positions;
    private Color[] colors;
    private float[] sizes;

    public int AliveCount => aliveCount;

    void Awake()
    {
        InitializePool();
    }

    public void InitializePool()
    {
        particles = new ParticleState[maxParticles];
        freeIndices = new int[maxParticles];
        positions = new Vector3[maxParticles];
        colors = new Color[maxParticles];
        sizes = new float[maxParticles];

        for (int i = 0; i < maxParticles; i++)
        {
            particles[i] = new ParticleState();
            particles[i].Reset();
            freeIndices[i] = i;
        }
        freeIndexHead = maxParticles;
        aliveCount = 0;
    }

    public void Emit(Vector3 origin, Vector3 direction, int count, int charge, float spread, float temperature)
    {
        for (int k = 0; k < count; k++)
        {
            if (freeIndexHead <= 0) break;

            int idx = freeIndices[--freeIndexHead];
            ParticleState p = particles[idx];
            p.alive = true;
            p.position = origin + Random.insideUnitSphere * spread;
            p.velocity = direction * Random.Range(0.5f, 1.5f) + Random.insideUnitSphere * spread * 2f;
            p.charge = charge;
            p.temperature = temperature + Random.Range(-200f, 200f);
            p.lifetime = particleLifetime * Random.Range(0.8f, 1.2f);
            aliveCount++;
        }
    }

    public void Simulate(float dt, FieldSolver fieldSolver, float coolingWind)
    {
        float halfDomain = domainSize * 0.5f;
        List<int> toRemove = new List<int>();

        for (int i = 0; i < maxParticles; i++)
        {
            ParticleState p = particles[i];
            if (!p.alive) continue;

            p.lifetime -= dt;
            if (p.lifetime <= 0f)
            {
                toRemove.Add(i);
                continue;
            }

            Vector3 eField = fieldSolver.SampleElectricField(p.position);
            float localTemp = fieldSolver.SampleTemperature(p.position);
            Vector3 bField = fieldSolver.SampleMagneticField(p.position);

            float mobility = p.charge < 0 ? electronMobility : ionMobility;
            Vector3 driftVelocity = p.charge * mobility * eField;

            float thermalVelocity = Mathf.Sqrt(2f * 1.38e-23f * localTemp / (p.charge < 0 ? 9.11e-31f : 6.64e-26f));
            thermalVelocity = Mathf.Min(thermalVelocity * 1e-6f, 5f);
            Vector3 randomWalk = Random.insideUnitSphere * thermalVelocity * dt;

            Vector3 windForce = Vector3.right * coolingWind * 0.01f;

            Vector3 lorentzAccel = p.charge * lorentzForceScale * Vector3.Cross(p.velocity + driftVelocity, bField)
                / (p.charge < 0 ? 9.11e-4f : 6.64e-3f);

            p.velocity = driftVelocity + randomWalk + windForce + lorentzAccel * dt;
            p.position += p.velocity * dt;

            p.temperature = Mathf.Lerp(p.temperature, localTemp, dt * 2f);

            if (Mathf.Abs(p.position.x) > halfDomain ||
                Mathf.Abs(p.position.z) > halfDomain)
            {
                toRemove.Add(i);
                continue;
            }
        }

        ProcessRecombination(toRemove);

        foreach (int idx in toRemove)
        {
            KillParticle(idx);
        }

        BuildRenderData();
    }

    void ProcessRecombination(List<int> toRemove)
    {
        int gridSize = 32;
        float cellSize = domainSize / gridSize;
        Dictionary<int, List<int>> grid = new Dictionary<int, List<int>>();

        for (int i = 0; i < maxParticles; i++)
        {
            ParticleState p = particles[i];
            if (!p.alive) continue;

            float h = domainSize * 0.5f;
            if (Mathf.Abs(p.position.x) > h || Mathf.Abs(p.position.z) > h)
            {
                if (!toRemove.Contains(i))
                    toRemove.Add(i);
                continue;
            }

            int ci = Mathf.FloorToInt((p.position.x + h) / cellSize);
            int cj = Mathf.FloorToInt((p.position.z + h) / cellSize);
            if (ci < 0 || ci >= gridSize || cj < 0 || cj >= gridSize)
            {
                if (!toRemove.Contains(i))
                    toRemove.Add(i);
                continue;
            }

            int key = ci * 1000 + cj;

            if (!grid.ContainsKey(key))
                grid[key] = new List<int>();
            grid[key].Add(i);
        }

        foreach (var cell in grid)
        {
            var indices = cell.Value;
            List<int> electrons = new List<int>();
            List<int> ions = new List<int>();

            foreach (int idx in indices)
            {
                if (particles[idx].charge < 0) electrons.Add(idx);
                else if (particles[idx].charge > 0) ions.Add(idx);
            }

            int pairs = Mathf.Min(electrons.Count, ions.Count);
            float recombProb = recombinationCoeff * 1e8f * Time.deltaTime;

            for (int p = 0; p < pairs; p++)
            {
                if (Random.value < recombProb)
                {
                    toRemove.Add(electrons[p]);
                    toRemove.Add(ions[p]);
                }
            }
        }
    }

    void KillParticle(int idx)
    {
        particles[idx].alive = false;
        freeIndices[freeIndexHead++] = idx;
        aliveCount--;
    }

    void BuildRenderData()
    {
        for (int i = 0; i < maxParticles; i++)
        {
            ParticleState p = particles[i];
            if (p.alive)
            {
                positions[i] = p.position;
                float tNorm = Mathf.Clamp01((p.temperature - ambientTemp) / 19700f);
                colors[i] = TemperatureToColor(tNorm, p.charge);
                sizes[i] = p.charge < 0 ? 0.003f : 0.005f;
            }
            else
            {
                positions[i] = Vector3.zero;
                colors[i] = Color.clear;
                sizes[i] = 0f;
            }
        }
    }

    Color TemperatureToColor(float tNorm, int charge)
    {
        Color c;
        if (tNorm < 0.33f)
        {
            c = Color.Lerp(Color.red, Color.yellow, tNorm * 3f);
        }
        else if (tNorm < 0.66f)
        {
            c = Color.Lerp(Color.yellow, Color.white, (tNorm - 0.33f) * 3f);
        }
        else
        {
            c = Color.Lerp(Color.white, new Color(0.6f, 0.6f, 1f), (tNorm - 0.66f) * 3f);
        }

        if (charge < 0)
        {
            c.b = Mathf.Min(c.b + 0.3f, 1f);
        }
        else if (charge > 0)
        {
            c.r = Mathf.Min(c.r + 0.2f, 1f);
        }

        return c;
    }

    public void GetRenderData(out Vector3[] pos, out Color[] col, out float[] sz)
    {
        pos = positions;
        col = colors;
        sz = sizes;
    }

    public ParticleState[] GetAllParticles()
    {
        return particles;
    }

    public void SetParticleState(int idx, Vector3 pos, Vector3 vel, float temp, float life, int charge, bool alive)
    {
        if (idx < 0 || idx >= maxParticles) return;
        particles[idx].position = pos;
        particles[idx].velocity = vel;
        particles[idx].temperature = temp;
        particles[idx].lifetime = life;
        particles[idx].charge = charge;
        particles[idx].alive = alive;
    }

    public void ClearAll()
    {
        for (int i = 0; i < maxParticles; i++)
        {
            particles[i].Reset();
        }
        freeIndexHead = maxParticles;
        for (int i = 0; i < maxParticles; i++)
            freeIndices[i] = i;
        aliveCount = 0;
    }
}
