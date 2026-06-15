using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class PlasmaParticleRenderer : MonoBehaviour
{
    public Material arcGlowMaterial;
    public float baseParticleSize = 0.003f;

    private ParticleSystem ps;
    private ParticleSystem.Particle[] renderParticles;
    private PlasmaParticleSystem plasmaSystem;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        InitializeParticleSystem();
    }

    void InitializeParticleSystem()
    {
        var main = ps.main;
        main.maxParticles = 80000;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.startSpeed = 0f;
        main.startLifetime = Mathf.Infinity;
        main.startSize = baseParticleSize;

        var emission = ps.emission;
        emission.enabled = false;

        var shape = ps.shape;
        shape.enabled = false;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        if (arcGlowMaterial != null)
            renderer.material = arcGlowMaterial;
    }

    public void SetPlasmaSystem(PlasmaParticleSystem system)
    {
        plasmaSystem = system;
        renderParticles = new ParticleSystem.Particle[system.AliveCount > 0 ? 80000 : 1];
    }

    void LateUpdate()
    {
        if (plasmaSystem == null) return;

        plasmaSystem.GetRenderData(out Vector3[] pos, out Color[] col, out float[] sz);

        int maxP = pos.Length;
        if (renderParticles == null || renderParticles.Length != maxP)
            renderParticles = new ParticleSystem.Particle[maxP];

        int count = 0;
        for (int i = 0; i < maxP; i++)
        {
            if (col[i].a > 0f)
            {
                renderParticles[count].position = pos[i];
                renderParticles[count].startColor = col[i];
                renderParticles[count].startSize = sz[i];
                renderParticles[count].startLifetime = 1f;
                renderParticles[count].remainingLifetime = 1f;
                count++;
            }
        }

        ps.SetParticles(renderParticles, count);
    }
}
