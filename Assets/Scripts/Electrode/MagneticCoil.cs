using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class MagneticCoil : MonoBehaviour
{
    public float radius = 0.1f;
    public float tubeRadius = 0.008f;
    public int radialSegments = 48;
    public int tubularSegments = 12;
    public float current = 100f;
    public int turns = 10;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private CoilData data;

    public CoilData Data
    {
        get
        {
            if (data == null)
                data = new CoilData(transform.position, transform.up, radius, current, turns);
            return data;
        }
    }

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        GenerateTorusMesh();
    }

    public void GenerateTorusMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "MagneticCoil_Torus";

        int vertCount = (radialSegments + 1) * (tubularSegments + 1);
        int triCount = radialSegments * tubularSegments * 2;
        Vector3[] vertices = new Vector3[vertCount];
        Vector3[] normals = new Vector3[vertCount];
        Vector2[] uv = new Vector2[vertCount];
        int[] triangles = new int[triCount * 3];

        int vi = 0;
        for (int j = 0; j <= tubularSegments; j++)
        {
            float v = (float)j / tubularSegments;
            float tubeAngle = v * 2f * Mathf.PI;
            float cosT = Mathf.Cos(tubeAngle);
            float sinT = Mathf.Sin(tubeAngle);

            for (int i = 0; i <= radialSegments; i++)
            {
                float u = (float)i / radialSegments;
                float ringAngle = u * 2f * Mathf.PI;
                float cosR = Mathf.Cos(ringAngle);
                float sinR = Mathf.Sin(ringAngle);

                float ringX = radius + tubeRadius * cosT;
                float x = ringX * cosR;
                float y = tubeRadius * sinT;
                float z = ringX * sinR;

                Vector3 center = new Vector3(cosR * radius, 0f, sinR * radius);
                Vector3 normal = new Vector3(x, y, z) - center;
                normal.Normalize();

                vertices[vi] = new Vector3(x, y, z);
                normals[vi] = normal;
                uv[vi] = new Vector2(u, v);
                vi++;
            }
        }

        int ti = 0;
        for (int j = 0; j < tubularSegments; j++)
        {
            for (int i = 0; i < radialSegments; i++)
            {
                int a = j * (radialSegments + 1) + i;
                int b = a + radialSegments + 1;
                int c = a + 1;
                int d = b + 1;

                triangles[ti++] = a;
                triangles[ti++] = b;
                triangles[ti++] = c;

                triangles[ti++] = b;
                triangles[ti++] = d;
                triangles[ti++] = c;
            }
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

        data = new CoilData(transform.position, transform.up, radius, current, turns);
    }

    public void UpdateCurrent(float newCurrent)
    {
        current = newCurrent;
        if (data != null)
            data.current = newCurrent;
    }

    public void UpdateTurns(int newTurns)
    {
        turns = Mathf.Max(1, newTurns);
        if (data != null)
            data.turns = turns;
    }

    public void SyncTransform()
    {
        if (data != null)
        {
            data.position = transform.position;
            data.axisDirection = transform.up.normalized;
            data.radius = radius;
            data.current = current;
            data.turns = turns;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        int seg = 64;
        Vector3 prev = Vector3.zero;
        for (int i = 0; i <= seg; i++)
        {
            float t = (float)i / seg * 2f * Mathf.PI;
            Vector3 p = new Vector3(Mathf.Cos(t) * radius, 0f, Mathf.Sin(t) * radius);
            if (i > 0) Gizmos.DrawLine(prev, p);
            prev = p;
        }
        Gizmos.DrawIcon(transform.position, "", false);
    }
}
