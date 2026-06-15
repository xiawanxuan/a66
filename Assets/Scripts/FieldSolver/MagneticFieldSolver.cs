using UnityEngine;
using System.Collections.Generic;

public class MagneticFieldSolver : MonoBehaviour
{
    public int gridResolution = 128;
    public float domainSize = 1f;
    public int biotSavartSegments = 96;
    public float vacuumPermeability = 1.25663706212e-6f;

    private Vector3[] magneticFieldGrid;
    private float cellSize;

    public Vector3[] MagneticField => magneticFieldGrid;
    public float CellSize => cellSize;

    void Awake()
    {
        InitializeGrid();
    }

    public void InitializeGrid()
    {
        int n = gridResolution;
        magneticFieldGrid = new Vector3[n * n];
        cellSize = domainSize / n;
        System.Array.Clear(magneticFieldGrid, 0, n * n);
    }

    public void SolveMagneticField(List<CoilData> coils)
    {
        if (magneticFieldGrid == null || magneticFieldGrid.Length != gridResolution * gridResolution)
            InitializeGrid();

        int n = gridResolution;
        System.Array.Clear(magneticFieldGrid, 0, n * n);

        if (coils == null || coils.Count == 0) return;

        float cs = cellSize;
        float halfDomain = domainSize * 0.5f;
        float mu0_4pi = vacuumPermeability * 0.25f / Mathf.PI;

        Vector3[] segPositions = new Vector3[biotSavartSegments];
        Vector3[] segDl = new Vector3[biotSavartSegments];

        foreach (var coil in coils)
        {
            if (coil.current == 0f || coil.turns == 0 || coil.radius <= 0f) continue;

            ComputeCoilSegments(coil, segPositions, segDl);

            float effectiveCurrent = coil.current * coil.turns;

            for (int j = 0; j < n; j++)
            {
                float z = j * cs - halfDomain;
                for (int i = 0; i < n; i++)
                {
                    float x = i * cs - halfDomain;
                    Vector3 p = new Vector3(x, 0f, z);

                    Vector3 b = BiotSavartIntegrate(p, segPositions, segDl, mu0_4pi * effectiveCurrent);
                    magneticFieldGrid[j * n + i] += b;
                }
            }
        }
    }

    void ComputeCoilSegments(CoilData coil, Vector3[] positions, Vector3[] dl)
    {
        Vector3 axis = coil.axisDirection.normalized;
        Vector3 radial;
        if (Mathf.Abs(Vector3.Dot(axis, Vector3.right)) < 0.9f)
            radial = Vector3.Cross(axis, Vector3.right).normalized;
        else
            radial = Vector3.Cross(axis, Vector3.forward).normalized;
        Vector3 tangential = Vector3.Cross(axis, radial).normalized;

        float dAngle = 2f * Mathf.PI / biotSavartSegments;
        float radius = coil.radius;

        for (int s = 0; s < biotSavartSegments; s++)
        {
            float a = s * dAngle;
            float cosA = Mathf.Cos(a);
            float sinA = Mathf.Sin(a);

            positions[s] = coil.position + (radial * cosA + tangential * sinA) * radius;

            Vector3 nextP = coil.position + (radial * Mathf.Cos(a + dAngle) + tangential * Mathf.Sin(a + dAngle)) * radius;
            dl[s] = nextP - positions[s];
        }
    }

    Vector3 BiotSavartIntegrate(Vector3 p, Vector3[] segPos, Vector3[] segDl, float prefactor)
    {
        Vector3 b = Vector3.zero;
        for (int s = 0; s < biotSavartSegments; s++)
        {
            Vector3 r = p - segPos[s];
            float rMag = r.magnitude;
            if (rMag < 1e-6f) continue;

            float r3 = rMag * rMag * rMag;
            Vector3 cross = Vector3.Cross(segDl[s], r);
            b += prefactor * cross / r3;
        }
        return b;
    }

    public Vector3 SampleMagneticField(Vector3 worldPos)
    {
        if (magneticFieldGrid == null) return Vector3.zero;

        int n = gridResolution;
        float cs = cellSize;
        float halfDomain = domainSize * 0.5f;

        int i = Mathf.Clamp((int)((worldPos.x + halfDomain) / cs), 0, n - 1);
        int j = Mathf.Clamp((int)((worldPos.z + halfDomain) / cs), 0, n - 1);

        return magneticFieldGrid[j * n + i];
    }

    public float GetMaxFieldMagnitude()
    {
        if (magneticFieldGrid == null) return 0f;
        float maxB = 0f;
        for (int i = 0; i < magneticFieldGrid.Length; i++)
        {
            float m = magneticFieldGrid[i].magnitude;
            if (m > maxB) maxB = m;
        }
        return maxB;
    }

    public void GetFieldData(out Vector3[] bField)
    {
        bField = magneticFieldGrid;
    }

    public void SetFieldData(Vector3[] bField)
    {
        if (bField == null) return;
        if (magneticFieldGrid == null || magneticFieldGrid.Length != bField.Length)
            magneticFieldGrid = new Vector3[bField.Length];
        System.Array.Copy(bField, magneticFieldGrid, bField.Length);
    }
}
