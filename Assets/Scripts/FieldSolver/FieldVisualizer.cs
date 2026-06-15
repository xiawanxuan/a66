using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FieldVisualizer : MonoBehaviour
{
    public FieldSolver fieldSolver;
    public bool showPotential = true;
    public bool showElectricField = true;
    public bool showTemperature = true;
    public float fieldArrowScale = 0.02f;

    private MeshFilter meshFilter;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    public void GenerateFieldVisualization()
    {
        if (fieldSolver == null) return;

        int n = fieldSolver.gridResolution;
        float cs = fieldSolver.CellSize;
        float halfDomain = fieldSolver.domainSize * 0.5f;

        if (showPotential)
        {
            GeneratePotentialMesh(n, cs, halfDomain);
        }
    }

    void GeneratePotentialMesh(int n, float cs, float halfDomain)
    {
        int vertCount = n * n;
        Vector3[] vertices = new Vector3[vertCount];
        Color[] colors = new Color[vertCount];
        int[] triangles = new int[(n - 1) * (n - 1) * 6];

        float[] potential = fieldSolver.Potential;
        float maxPot = 0f;
        foreach (float p in potential)
        {
            if (Mathf.Abs(p) > maxPot) maxPot = Mathf.Abs(p);
        }
        if (maxPot < 1f) maxPot = 1f;

        for (int j = 0; j < n; j++)
        {
            for (int i = 0; i < n; i++)
            {
                int idx = j * n + i;
                float x = i * cs - halfDomain;
                float z = j * cs - halfDomain;
                float y = potential[idx] / maxPot * 0.05f;

                vertices[idx] = new Vector3(x, y, z);

                float t = potential[idx] / maxPot;
                colors[idx] = new Color(t, 0.2f, 1f - t, 0.5f);
            }
        }

        int triIdx = 0;
        for (int j = 0; j < n - 1; j++)
        {
            for (int i = 0; i < n - 1; i++)
            {
                int v0 = j * n + i;
                int v1 = v0 + 1;
                int v2 = (j + 1) * n + i;
                int v3 = v2 + 1;

                triangles[triIdx++] = v0;
                triangles[triIdx++] = v2;
                triangles[triIdx++] = v1;

                triangles[triIdx++] = v1;
                triangles[triIdx++] = v2;
                triangles[triIdx++] = v3;
            }
        }

        Mesh mesh = new Mesh();
        mesh.name = "FieldVisualization";
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    void OnDrawGizmos()
    {
        if (fieldSolver == null || !showElectricField) return;

        Vector3[] eField = fieldSolver.ElectricField;
        int n = fieldSolver.gridResolution;
        float cs = fieldSolver.CellSize;
        float halfDomain = fieldSolver.domainSize * 0.5f;
        int step = Mathf.Max(1, n / 16);

        Gizmos.color = Color.yellow;
        for (int j = 0; j < n; j += step)
        {
            for (int i = 0; i < n; i += step)
            {
                int idx = j * n + i;
                Vector3 pos = new Vector3(i * cs - halfDomain, 0.01f, j * cs - halfDomain);
                Vector3 e = eField[idx] * fieldArrowScale;
                Gizmos.DrawLine(pos, pos + e);
            }
        }
    }
}
