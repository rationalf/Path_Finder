using UnityEngine;
using System.Collections.Generic;

public class HexGrid : MonoBehaviour
{
    public GameObject hexPrefab; // Assign your Hexagon prefab here
    public int gridWidth = 100;   // Number of hexagons horizontally
    public int gridHeight = 100;  // Number of hexagons vertically
    public float hexRadius = 1f; // Radius of the hexagon (distance from center to corner)
    public int numSites = 5; // Number of Voronoi sites (regions)
    public Color[] regionColors; // Colors for each Voronoi region
    public float heightFactor = 2f; // How much elevation variation there is
    public float perlinScale = 0.1f; // Scale for Perlin noise
    public Gradient heightGradient; // Gradient to map height to color

    private List<Vector2> sides; // List to store the Voronoi sites
    public List<GameObject> hexes; // List to store all hexagons
    public bool isEndSelected = false;
    
    void Start()
    {
        float hexWidth = Mathf.Sqrt(2) * hexRadius; // Horizontal distance between centers (flat sides)
        float hexHeight = Mathf.Sqrt(3) * hexRadius; // Vertical distance between centers (point to point)
        float rowOffset = hexHeight * 0.5f; // Vertical offset for staggered rows (half height)

        sides = new List<Vector2>();
        hexes = new List<GameObject>();
        GenerateVoronoiSites();

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                float xPos = x * hexWidth;
                float zPos = y * hexHeight + (x % 2 == 0 ? 0 : rowOffset);

                float height = Mathf.PerlinNoise(xPos * perlinScale, zPos * perlinScale) * heightFactor;
                Quaternion hexRotation = Quaternion.Euler(180f, 0f, 0f);
                Vector3 hexPosition = new Vector3(xPos, height, zPos);

                GameObject hexagon = Instantiate(hexPrefab, hexPosition, hexRotation, transform);
                hexagon.GetComponent<HexagonalPrismGenerator>().index = hexes.Count;
                hexes.Add(hexagon); // Store hexagon in the list
                hexagon.AddComponent<MeshCollider>();
                float normalizedHeight = Mathf.InverseLerp(0, heightFactor, height);
                Color hexColor = heightGradient.Evaluate(normalizedHeight);

                Renderer hexRenderer = hexagon.GetComponent<Renderer>();
                if (hexRenderer != null)
                {
                    hexRenderer.material.color = hexColor;
                }

                Vector2 closestSite = GetClosestSite(new Vector2(xPos, zPos));
                int regionIndex = sides.IndexOf(closestSite);
                if (regionIndex >= 0 && regionIndex < regionColors.Length)
                {
                    hexRenderer.material.color = Color.Lerp(hexColor, regionColors[regionIndex], 0.5f);
                }
            }
        }
        
    }
    
    // Generate random Voronoi sites within the grid bounds
    private void GenerateVoronoiSites()
    {
        float hexWidth = Mathf.Sqrt(2) * hexRadius;
        float hexHeight = Mathf.Sqrt(3) * hexRadius;

        for (int i = 0; i < numSites; i++)
        {
            float x = Random.Range(0, gridWidth) * hexWidth;
            float z = Random.Range(0, gridHeight) * hexHeight;
            sides.Add(new Vector2(x, z));
        }
    }

    // Get the closest Voronoi site to a given position
    private Vector2 GetClosestSite(Vector2 position)
    {
        float minDistance = float.MaxValue;
        Vector2 closestSite = Vector2.zero;

        foreach (var site in sides)
        {
            float distance = Vector2.Distance(position, site);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestSite = site;
            }
        }

        return closestSite;
    }

    // Expose all created hexagons
    public IEnumerable<GameObject> GetHexes()
    {
        return hexes;
    }
}
