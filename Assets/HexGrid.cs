using UnityEngine;
using System.Collections.Generic;

public class HexGrid : MonoBehaviour
{
    public GameObject hexPrefab; // Assign your Hexagon prefab here
    public int gridWidth = 10;   // Number of hexagons horizontally
    public int gridHeight = 10;  // Number of hexagons vertically
    public float hexRadius = 1f; // Radius of the hexagon (distance from center to corner)
    public int numSites = 5; // Number of Voronoi sites (regions)
    public Color[] regionColors; // Colors for each Voronoi region
    public float heightFactor = 2f; // How much elevation variation there is
    public float perlinScale = 0.1f; // Scale for Perlin noise
    public Gradient heightGradient; // Gradient to map height to color
    
    private List<Vector2> sites; // List to store the Voronoi sites

    void Start()
    {
        // Calculate the spacing for side-to-side connection
        float hexWidth = Mathf.Sqrt(2) * hexRadius; // Horizontal distance between centers (flat sides)
        float hexHeight = Mathf.Sqrt(3) * hexRadius; // Vertical distance between centers (point to point)
        float rowOffset = hexHeight * 0.5f; // Vertical offset for staggered rows (half height)

        // Initialize the list of Voronoi sites
        sites = new List<Vector2>();
        GenerateVoronoiSites();

        // Loop through rows and columns to instantiate hexagons
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                // Calculate position for each hexagon, staggering every other row
                float xPos = x * hexWidth;
                float zPos = y * hexHeight + (x % 2 == 0 ? 0 : rowOffset);

                // Apply elevation using Perlin noise
                float height = Mathf.PerlinNoise(xPos * perlinScale, zPos * perlinScale) * heightFactor;

                // Apply 180-degree rotation along the X-axis
                Quaternion hexRotation = Quaternion.Euler(180f, 0f, 0f);

                // Instantiate the hexagon at the calculated position with the rotation
                Vector3 hexPosition = new Vector3(xPos, height, zPos);
                GameObject hexagon = Instantiate(hexPrefab, hexPosition, hexRotation, transform);
                
                // Normalize height to fit between 0 and 1
                float normalizedHeight = Mathf.InverseLerp(0, heightFactor, height);
                // Apply color from the gradient based on normalized height
                Color hexColor = heightGradient.Evaluate(normalizedHeight); // Get color from gradient

                // Apply color to the hexagon's material
                Renderer hexRenderer = hexagon.GetComponent<Renderer>();
                if (hexRenderer != null)
                {
                    hexRenderer.material.color = hexColor;
                }

                // Optionally, assign Voronoi region-based color
                Vector2 closestSite = GetClosestSite(new Vector2(xPos, zPos));
                int regionIndex = sites.IndexOf(closestSite);
                if (regionIndex >= 0 && regionIndex < regionColors.Length)
                {
                    // You can mix or override color based on the Voronoi region if desired
                    // Example: Add a secondary color effect based on Voronoi region
                    hexRenderer.material.color = Color.Lerp(hexColor, regionColors[regionIndex], 0.5f);
                }
            }
        }
    }

    // Generate random Voronoi sites within the grid bounds
    private void GenerateVoronoiSites()
    {
        float hexWidth = Mathf.Sqrt(2) * hexRadius; // Horizontal distance between centers
        float hexHeight = Mathf.Sqrt(3) * hexRadius; // Vertical distance between centers

        // Generate random sites within the grid area
        for (int i = 0; i < numSites; i++)
        {
            float x = Random.Range(0, gridWidth) * hexWidth;
            float z = Random.Range(0, gridHeight) * hexHeight;
            sites.Add(new Vector2(x, z));
        }
    }

    // Get the closest Voronoi site to a given position
    private Vector2 GetClosestSite(Vector2 position)
    {
        float minDistance = float.MaxValue;
        Vector2 closestSite = Vector2.zero;

        foreach (var site in sites)
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
}
