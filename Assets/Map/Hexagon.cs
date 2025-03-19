using UnityEngine;
//Class for creating hexagons for map
public class HexagonalPrismGenerator : MonoBehaviour
{
    public float radius = 1f;
    public float height = 2f;
    public int index;
    public Mesh mesh1;
    public float height1;
    void Start()
    {
        GenerateHexagonalPrism();
    }

    void GenerateHexagonalPrism()
    {
        Mesh mesh = new Mesh();
        
        Vector3[] vertices = new Vector3[12];

        float angleStep = 60f;

        for (int i = 0; i < 6; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            vertices[i] = new Vector3(x, height / 2f, z);
            vertices[i + 6] = new Vector3(x, (-height / 2f) - 10, z);
        }

        int[] triangles = new int[36];

        for (int i = 0; i < 6; i++)
        {
            int next = (i + 1) % 6;

            triangles[i * 6] = i;
            triangles[i * 6 + 1] = next;
            triangles[i * 6 + 2] = i + 6;

            triangles[i * 6 + 3] = next;
            triangles[i * 6 + 4] = next + 6;
            triangles[i * 6 + 5] = i + 6;
        }

        int[] topTriangles = new int[18];
        for (int i = 0; i < 6; i++)
        {
            int next = (i + 1) % 6;
            topTriangles[i * 3] = 0; 
            topTriangles[i * 3 + 1] = i; 
            topTriangles[i * 3 + 2] = next;
        }

        int[] bottomTriangles = new int[18];
        for (int i = 0; i < 6; i++)
        {
            int next = (i + 1) % 6;
            bottomTriangles[i * 3] = 6; 
            bottomTriangles[i * 3 + 1] = i + 6;
            bottomTriangles[i * 3 + 2] = next + 6;
        }

        int[] allTriangles = new int[36 + 18 + 18];
        topTriangles.CopyTo(allTriangles, 0);
        bottomTriangles.CopyTo(allTriangles, topTriangles.Length);
        triangles.CopyTo(allTriangles, topTriangles.Length + bottomTriangles.Length);
        mesh.vertices = vertices;
        mesh.triangles = allTriangles;

        GetComponent<MeshFilter>().mesh = mesh;
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        Material defaultMaterial = new Material(Shader.Find("Standard"));
        GetComponent<MeshCollider>().sharedMesh = mesh;
        GetComponent<MeshCollider>().convex = true;
    }
}
