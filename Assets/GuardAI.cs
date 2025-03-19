using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GuardAI : MonoBehaviour
{
    public HexGrid hexGrid;
    public float speed = 5f;
    public List<Vector3> path;
    private int currentWaypointIndex = 0;
    private bool pathGenerated = false;

    void Start()
    {
        path = new List<Vector3>();
    }

    void Update()
    {
        if (!pathGenerated && hexGrid.created)
        {
            OnMapGenerated();
            pathGenerated = true;
        }

        if (path == null || path.Count == 0) return;

        // ✅ Двигаемся к следующему hex
        Vector3 targetPosition = path[currentWaypointIndex];
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            // ⏭️ Переход к следующему hex по кругу
            currentWaypointIndex = (currentWaypointIndex + 1) % path.Count;
        }
    }

    void OnMapGenerated()
    {
        GeneratePath();
        if (path.Count > 0)
        {
            TeleportToFirstHex();
        }
        else
        {
            Debug.LogError("❌ No valid path found!");
        }
    }

    void GeneratePath()
    {
        path = new List<Vector3>();

        // 🔍 Находим граничные гексы по Voronoi-сегментам
        List<GameObject> boundaryHexes = GetBoundaryHexes();

        Debug.Log($"🧐 Found {boundaryHexes.Count} boundary hexes.");

        if (boundaryHexes.Count == 0)
        {
            Debug.LogError("❌ No boundary hexes found!");
            return;
        }

        // 🌀 Упорядочиваем по часовой стрелке
        boundaryHexes = SortHexesClockwise(boundaryHexes);

        foreach (var hex in boundaryHexes)
        {
            float height = hex.GetComponent<HexagonalPrismGenerator>().height1;
            Vector3 hexPosition = hex.transform.position;
            path.Add(new Vector3(hexPosition.x, height + 11, hexPosition.z));
            Debug.Log($"➡️ Added hex at {hexPosition} to path");
        }

        Debug.Log($"✅ Generated path with {path.Count} waypoints.");
    }


    List<GameObject> GetBoundaryHexes()
    {
        List<GameObject> boundaryHexes = new List<GameObject>();

        foreach (var hex in hexGrid.hexes)
        {
            int neighboringRegions = GetNeighboringRegions(hex);
            if (neighboringRegions > 1 || GetNeighbors(hex).Count < 6)
            {
                boundaryHexes.Add(hex);
            }
        }

        Debug.Log($"✅ Found {boundaryHexes.Count} boundary hexes.");
        return boundaryHexes;
    }


    int GetNeighboringRegions(GameObject hex)
    {
        HashSet<Vector2> uniqueRegions = new HashSet<Vector2>();

        foreach (var neighbor in GetNeighbors(hex))
        {
            if (neighbor == null) 
            {
                // 🌟 Считаем пустоту как отдельный регион
                uniqueRegions.Add(Vector2.zero);
                continue;
            }

            Vector2 closestSite = hexGrid.GetClosestSite(new Vector2(neighbor.transform.position.x, neighbor.transform.position.z));
            uniqueRegions.Add(closestSite);
        }

        if (uniqueRegions.Count > 1)
        {
            Debug.Log($"✅ Hex at {hex.transform.position} borders {uniqueRegions.Count} regions.");
        }

        return uniqueRegions.Count;
    }

    List<GameObject> GetNeighbors(GameObject hex)
    {
        List<GameObject> neighbors = new List<GameObject>();
        Vector2 hexPosition = new Vector2(hex.transform.position.x, hex.transform.position.z);

        foreach (var potentialNeighbor in hexGrid.hexes)
        {
            if (potentialNeighbor == hex) continue;

            Vector2 neighborPosition = new Vector2(potentialNeighbor.transform.position.x, potentialNeighbor.transform.position.z);
            float distance = Vector2.Distance(hexPosition, neighborPosition);

            if (distance < hexGrid.hexRadius * 1.8f) // ✅ Убрали Y, только X и Z
            {
                Vector2 hexRegion = hexGrid.GetClosestSite(hexPosition);
                Vector2 neighborRegion = hexGrid.GetClosestSite(neighborPosition);

                if (hexRegion == neighborRegion)
                {
                    neighbors.Add(potentialNeighbor);
                }
            }
        }

        Debug.Log($"🧭 {neighbors.Count} valid neighbors found for hex at {hexPosition}");
        return neighbors;
    }



    List<GameObject> SortHexesClockwise(List<GameObject> hexes)
    {
        if (hexes.Count == 0) return hexes;

        List<GameObject> sortedHexes = new List<GameObject>();
        HashSet<GameObject> visited = new HashSet<GameObject>();

        GameObject current = hexes[0];
        sortedHexes.Add(current);
        visited.Add(current);

        while (sortedHexes.Count < hexes.Count)
        {
            var neighbors = GetNeighbors(current)
                .Where(n => hexes.Contains(n) && !visited.Contains(n))
                .OrderBy(n =>
                {
                    Vector2 dir = new Vector2(n.transform.position.x - current.transform.position.x,
                        n.transform.position.z - current.transform.position.z);
                    return Mathf.Atan2(dir.y, dir.x); // ✅ Считаем угол только по XZ
                })
                .ToList();

            if (neighbors.Count > 0)
            {
                current = neighbors[0];
                sortedHexes.Add(current);
                visited.Add(current);
            }
            else
            {
                break;
            }
        }

        return sortedHexes;
    }



    void TeleportToFirstHex()
    {
        if (path.Count == 0) return;

        transform.position = path[0];
        currentWaypointIndex = 0;
        Debug.Log($"🚀 Teleported to starting hex at: {path[0]}");
    }
}
