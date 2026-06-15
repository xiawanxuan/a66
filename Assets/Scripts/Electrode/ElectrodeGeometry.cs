using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ElectrodeGeometry : MonoBehaviour
{
    public ElectrodeType electrodeType = ElectrodeType.Cathode;
    public float radius = 0.02f;
    public float length = 0.05f;
    public int segments = 16;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private ElectrodeData data;

    public ElectrodeData Data
    {
        get
        {
            if (data == null)
            {
                float potential = electrodeType == ElectrodeType.Cathode ? 0f : 10000f;
                data = new ElectrodeData(electrodeType, transform.position, radius, potential);
            }
            return data;
        }
    }

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        GenerateMesh();
    }

    public void GenerateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = electrodeType + "_Electrode";

        int vertCount = (segments + 1) * 2 + 2;
        Vector3[] vertices = new Vector3[vertCount];
        Vector3[] normals = new Vector3[vertCount];
        Vector2[] uv = new Vector2[vertCount];

        float halfLen = length * 0.5f;

        for (int i = 0; i <= segments; i++)
        {
            float angle = (2f * Mathf.PI * i) / segments;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            vertices[i] = new Vector3(cos * radius, halfLen, sin * radius);
            normals[i] = new Vector3(cos, 0f, sin);
            uv[i] = new Vector2((float)i / segments, 1f);

            vertices[segments + 1 + i] = new Vector3(cos * radius, -halfLen, sin * radius);
            normals[segments + 1 + i] = new Vector3(cos, 0f, sin);
            uv[segments + 1 + i] = new Vector2((float)i / segments, 0f);
        }

        int topCenter = vertCount - 2;
        int botCenter = vertCount - 1;
        vertices[topCenter] = new Vector3(0f, halfLen, 0f);
        normals[topCenter] = Vector3.up;
        uv[topCenter] = new Vector2(0.5f, 1f);
        vertices[botCenter] = new Vector3(0f, -halfLen, 0f);
        normals[botCenter] = Vector3.down;
        uv[botCenter] = new Vector2(0.5f, 0f);

        int triCount = segments * 4;
        int[] triangles = new int[triCount * 3];
        int idx = 0;

        for (int i = 0; i < segments; i++)
        {
            int cur = i;
            int next = i + 1;
            int curBot = segments + 1 + i;
            int nextBot = segments + 1 + i + 1;

            triangles[idx++] = cur;
            triangles[idx++] = curBot;
            triangles[idx++] = next;

            triangles[idx++] = next;
            triangles[idx++] = curBot;
            triangles[idx++] = nextBot;
        }

        for (int i = 0; i < segments; i++)
        {
            int cur = i;
            int next = i + 1;

            triangles[idx++] = topCenter;
            triangles[idx++] = next;
            triangles[idx++] = cur;

            triangles[idx++] = botCenter;
            triangles[idx++] = segments + 1 + cur;
            triangles[idx++] = segments + 1 + next;
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

        data = new ElectrodeData(electrodeType, transform.position, radius, electrodeType == ElectrodeType.Cathode ? 0f : 10000f);
    }

    public void UpdatePotential(float voltage)
    {
        if (data != null)
        {
            data.potential = electrodeType == ElectrodeType.Cathode ? 0f : voltage;
        }
    }

    public void SyncPosition()
    {
        if (data != null)
        {
            data.position = transform.position;
            data.radius = radius;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = electrodeType == ElectrodeType.Cathode ? Color.blue : Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
