using UnityEngine;
using System.Collections.Generic;

public class FieldSolver : MonoBehaviour
{
    public int gridResolution = 128;
    public float domainSize = 1f;
    public int sorIterations = 50;
    public float sorOmega = 1.7f;

    private float[] potentialGrid;
    private Vector3[] electricFieldGrid;
    private float[] temperatureGrid;
    private float cellSize;

    public MagneticFieldSolver magneticFieldSolver;

    public float[] Potential => potentialGrid;
    public Vector3[] ElectricField => electricFieldGrid;
    public float[] Temperature => temperatureGrid;
    public float CellSize => cellSize;

    void Awake()
    {
        InitializeGrids();
    }

    public void InitializeGrids()
    {
        int n = gridResolution;
        potentialGrid = new float[n * n];
        electricFieldGrid = new Vector3[n * n];
        temperatureGrid = new float[n * n];
        cellSize = domainSize / n;

        for (int i = 0; i < n * n; i++)
        {
            temperatureGrid[i] = 300f;
        }
    }

    public void SolveElectricField(List<ElectrodeData> electrodes, float voltage)
    {
        int n = gridResolution;
        float cs = cellSize;

        for (int i = 0; i < n * n; i++)
            potentialGrid[i] = 0f;

        for (int iter = 0; iter < sorIterations; iter++)
        {
            for (int j = 1; j < n - 1; j++)
            {
                for (int i = 1; i < n - 1; i++)
                {
                    int idx = j * n + i;
                    float phiL = potentialGrid[idx - 1];
                    float phiR = potentialGrid[idx + 1];
                    float phiB = potentialGrid[(j - 1) * n + i];
                    float phiT = potentialGrid[(j + 1) * n + i];

                    float phiNew = (phiL + phiR + phiB + phiT) * 0.25f;
                    potentialGrid[idx] = potentialGrid[idx] + sorOmega * (phiNew - potentialGrid[idx]);
                }
            }

            ApplyElectrodeBoundary(electrodes, voltage);
        }

        ComputeElectricFieldGradient();
    }

    void ApplyElectrodeBoundary(List<ElectrodeData> electrodes, float voltage)
    {
        int n = gridResolution;
        float cs = cellSize;

        foreach (var elec in electrodes)
        {
            float cx = elec.position.x;
            float cz = elec.position.z;
            float r = elec.radius * 2f;

            int minI = Mathf.Clamp((int)((cx - r + domainSize * 0.5f) / cs), 0, n - 1);
            int maxI = Mathf.Clamp((int)((cx + r + domainSize * 0.5f) / cs), 0, n - 1);
            int minJ = Mathf.Clamp((int)((cz - r + domainSize * 0.5f) / cs), 0, n - 1);
            int maxJ = Mathf.Clamp((int)((cz + r + domainSize * 0.5f) / cs), 0, n - 1);

            for (int j = minJ; j <= maxJ; j++)
            {
                for (int i = minI; i <= maxI; i++)
                {
                    float wx = (i * cs - domainSize * 0.5f);
                    float wz = (j * cs - domainSize * 0.5f);
                    float dist = Mathf.Sqrt((wx - cx) * (wx - cx) + (wz - cz) * (wz - cz));

                    if (dist <= elec.radius * 1.5f)
                    {
                        float pot = elec.type == ElectrodeType.Cathode ? 0f : voltage;
                        potentialGrid[j * n + i] = pot;
                    }
                }
            }
        }
    }

    void ComputeElectricFieldGradient()
    {
        int n = gridResolution;
        float cs2 = 2f * cellSize;

        for (int j = 1; j < n - 1; j++)
        {
            for (int i = 1; i < n - 1; i++)
            {
                int idx = j * n + i;
                float dPhidx = (potentialGrid[idx + 1] - potentialGrid[idx - 1]) / cs2;
                float dPhidz = (potentialGrid[(j + 1) * n + i] - potentialGrid[(j - 1) * n + i]) / cs2;
                electricFieldGrid[idx] = new Vector3(-dPhidx, 0f, -dPhidz);
            }
        }

        for (int i = 0; i < n; i++)
        {
            electricFieldGrid[i] = electricFieldGrid[n + i];
            electricFieldGrid[(n - 1) * n + i] = electricFieldGrid[(n - 2) * n + i];
            electricFieldGrid[i * n] = electricFieldGrid[i * n + 1];
            electricFieldGrid[i * n + n - 1] = electricFieldGrid[i * n + n - 2];
        }
    }

    public void SolveThermalField(float dt, float coolingWind, float ambientTemp, float diffusivity, bool arcActive, Vector3 arcCenter, float arcRadius, float arcTemp)
    {
        int n = gridResolution;
        float cs = cellSize;
        float cs2 = cs * cs;

        float[] newTemp = new float[n * n];
        System.Array.Copy(temperatureGrid, newTemp, n * n);

        float stabilityLimit = cs2 / (4f * diffusivity);
        float effectiveDt = Mathf.Min(dt, stabilityLimit * 0.8f);

        for (int j = 1; j < n - 1; j++)
        {
            for (int i = 1; i < n - 1; i++)
            {
                int idx = j * n + i;
                float tL = temperatureGrid[idx - 1];
                float tR = temperatureGrid[idx + 1];
                float tB = temperatureGrid[(j - 1) * n + i];
                float tT = temperatureGrid[(j + 1) * n + i];
                float tC = temperatureGrid[idx];

                float laplacian = (tL + tR + tB + tT - 4f * tC) / cs2;
                float cooling = coolingWind > 0f ? coolingWind * (tC - ambientTemp) * 0.001f : 0f;

                newTemp[idx] = tC + effectiveDt * (diffusivity * laplacian - cooling);
                newTemp[idx] = Mathf.Max(newTemp[idx], ambientTemp);
            }
        }

        if (arcActive)
        {
            InjectArcHeat(newTemp, arcCenter, arcRadius, arcTemp, dt);
        }

        System.Array.Copy(newTemp, temperatureGrid, n * n);
    }

    void InjectArcHeat(float[] tempGrid, Vector3 arcCenter, float arcRadius, float arcTemp, float dt)
    {
        int n = gridResolution;
        float cs = cellSize;

        float cx = arcCenter.x;
        float cz = arcCenter.z;
        float r = arcRadius * 3f;

        int minI = Mathf.Clamp((int)((cx - r + domainSize * 0.5f) / cs), 0, n - 1);
        int maxI = Mathf.Clamp((int)((cx + r + domainSize * 0.5f) / cs), 0, n - 1);
        int minJ = Mathf.Clamp((int)((cz - r + domainSize * 0.5f) / cs), 0, n - 1);
        int maxJ = Mathf.Clamp((int)((cz + r + domainSize * 0.5f) / cs), 0, n - 1);

        for (int j = minJ; j <= maxJ; j++)
        {
            for (int i = minI; i <= maxI; i++)
            {
                float wx = (i * cs - domainSize * 0.5f);
                float wz = (j * cs - domainSize * 0.5f);
                float dist = Mathf.Sqrt((wx - cx) * (wx - cx) + (wz - cz) * (wz - cz));

                if (dist < arcRadius * 3f)
                {
                    float gaussian = Mathf.Exp(-dist * dist / (2f * arcRadius * arcRadius));
                    int idx = j * n + i;
                    tempGrid[idx] = Mathf.Lerp(tempGrid[idx], arcTemp, gaussian * dt * 5f);
                }
            }
        }
    }

    public Vector3 SampleElectricField(Vector3 worldPos)
    {
        int n = gridResolution;
        float cs = cellSize;
        int i = Mathf.Clamp((int)((worldPos.x + domainSize * 0.5f) / cs), 0, n - 1);
        int j = Mathf.Clamp((int)((worldPos.z + domainSize * 0.5f) / cs), 0, n - 1);
        return electricFieldGrid[j * n + i];
    }

    public float SamplePotential(Vector3 worldPos)
    {
        int n = gridResolution;
        float cs = cellSize;
        int i = Mathf.Clamp((int)((worldPos.x + domainSize * 0.5f) / cs), 0, n - 1);
        int j = Mathf.Clamp((int)((worldPos.z + domainSize * 0.5f) / cs), 0, n - 1);
        return potentialGrid[j * n + i];
    }

    public float SampleTemperature(Vector3 worldPos)
    {
        int n = gridResolution;
        float cs = cellSize;
        int i = Mathf.Clamp((int)((worldPos.x + domainSize * 0.5f) / cs), 0, n - 1);
        int j = Mathf.Clamp((int)((worldPos.z + domainSize * 0.5f) / cs), 0, n - 1);
        return temperatureGrid[j * n + i];
    }

    public float GetMaxFieldStrength()
    {
        float maxE = 0f;
        for (int i = 0; i < electricFieldGrid.Length; i++)
        {
            float mag = electricFieldGrid[i].magnitude;
            if (mag > maxE) maxE = mag;
        }
        return maxE;
    }

    public void GetFieldData(out float[] potential, out Vector3[] eField, out float[] temperature)
    {
        potential = potentialGrid;
        eField = electricFieldGrid;
        temperature = temperatureGrid;
    }

    public void SetFieldData(float[] potential, Vector3[] eField, float[] temperature)
    {
        System.Array.Copy(potential, potentialGrid, potentialGrid.Length);
        System.Array.Copy(eField, electricFieldGrid, electricFieldGrid.Length);
        System.Array.Copy(temperature, temperatureGrid, temperatureGrid.Length);
    }

    public Vector3 SampleMagneticField(Vector3 worldPos)
    {
        return magneticFieldSolver != null
            ? magneticFieldSolver.SampleMagneticField(worldPos)
            : Vector3.zero;
    }

    public Vector3[] GetMagneticFieldGrid()
    {
        return magneticFieldSolver != null ? magneticFieldSolver.MagneticField : null;
    }

    public void SetMagneticFieldData(Vector3[] bField)
    {
        if (magneticFieldSolver != null)
            magneticFieldSolver.SetFieldData(bField);
    }
}
