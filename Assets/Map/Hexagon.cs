using UnityEngine;

public class HexagonalPrismGenerator : MonoBehaviour
{
    public float radius = 1f; // Radius of the hexagon
    public float height = 2f; // Height of the hexagonal prism
    public int index;
    void Start()
    {
        GenerateHexagonalPrism();
    }

    void GenerateHexagonalPrism()
    {
        // Create a new mesh
        Mesh mesh = new Mesh();
        
        // Define the 6 vertices for the top and bottom hexagons
        Vector3[] vertices = new Vector3[12]; // 6 vertices for the top + 6 for the bottom

        // Angle between each vertex
        float angleStep = 60f;

        // Generate the top and bottom hexagonal vertices
        for (int i = 0; i < 6; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            // Top hexagon vertices
            vertices[i] = new Vector3(x, height / 2f, z);  // Adjust for height
            // Bottom hexagon vertices
            vertices[i + 6] = new Vector3(x, -height / 2f, z); // Adjust for bottom height
        }

        // Create the triangles for the hexagonal sides (lateral faces)
        int[] triangles = new int[36]; // 6 sides, each with 2 triangles, 3 vertices per triangle

        // For each side, create two triangles to form the lateral face
        for (int i = 0; i < 6; i++)
        {
            int next = (i + 1) % 6;

            // Top triangle
            triangles[i * 6] = i;
            triangles[i * 6 + 1] = next;
            triangles[i * 6 + 2] = i + 6;

            // Bottom triangle
            triangles[i * 6 + 3] = next;
            triangles[i * 6 + 4] = next + 6;
            triangles[i * 6 + 5] = i + 6;
        }

        // Create the top face (hexagon face)
        int[] topTriangles = new int[18]; // 6 triangles, each with 3 vertices
        for (int i = 0; i < 6; i++)
        {
            int next = (i + 1) % 6;
            topTriangles[i * 3] = 0;  // Center vertex (top)
            topTriangles[i * 3 + 1] = i; // Outer vertex i
            topTriangles[i * 3 + 2] = next; // Outer vertex next
        }

        // Create the bottom face (hexagon face)
        int[] bottomTriangles = new int[18]; // 6 triangles, each with 3 vertices
        for (int i = 0; i < 6; i++)
        {
            int next = (i + 1) % 6;
            bottomTriangles[i * 3] = 6;  // Center vertex (bottom)
            bottomTriangles[i * 3 + 1] = i + 6; // Outer vertex i (bottom)
            bottomTriangles[i * 3 + 2] = next + 6; // Outer vertex next (bottom)
        }

        // Combine all triangles into a single array
        int[] allTriangles = new int[36 + 18 + 18]; // Sides + top + bottom
        topTriangles.CopyTo(allTriangles, 0);
        bottomTriangles.CopyTo(allTriangles, topTriangles.Length);
        triangles.CopyTo(allTriangles, topTriangles.Length + bottomTriangles.Length);

        // Set the mesh properties
        mesh.vertices = vertices;
        mesh.triangles = allTriangles;

        // Apply the mesh to the MeshFilter component
        GetComponent<MeshFilter>().mesh = mesh;
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        Material defaultMaterial = new Material(Shader.Find("Standard"));
    }
}
