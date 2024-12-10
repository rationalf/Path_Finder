using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class PathfindingManager : MonoBehaviour
{
    public HexGrid hexGrid; // Map itself
    public GameObject startHex;
    public GameObject endHex;
    // dictionary for fast finding of neighbours
    private Dictionary<GameObject, List<GameObject>> hexNeighbors = new Dictionary<GameObject, List<GameObject>>();
    private List<GameObject> aStarPath;
    private int dijkstraOffset = 10;
    void Update()
    {
        if (hexGrid.isEndSelected)
        {
            startHex = HexagonSelector.startHex;
            endHex = HexagonSelector.endHex;
            StartCoroutine(DelayedPathfinding(1f));
        }

        hexGrid.isEndSelected = false;
    }

    private IEnumerator DelayedPathfinding(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log(hexGrid.hexes.Count);

        // Find the initial path using A*
        aStarPath  = AStar(startHex, endHex);
        //Visualize paths if possible
        if (aStarPath  != null)
        {
            var shortenedPath = aStarPath.ToArray()[..Mathf.Min(dijkstraOffset, aStarPath.Count)].ToList();
            StartCoroutine(VisualizeAStar(shortenedPath, 0.5f, Color.magenta));
            Debug.Log("A* cost:"+PathCost(shortenedPath));
        }
        else 
        {
            Debug.Log("A* path not found");
            List<GameObject> dijkstra = Dijkstra(startHex, endHex);
            if (dijkstra != null)
            {
                StartCoroutine(VisualizeDijkstra(dijkstra, 0.5f, Color.red));
                Debug.Log("Dijkstra cost:"+PathCost(dijkstra));
            }
            else
            {
                Debug.Log("Dijkstra path not found");
            }
        }
    }

    

    private IEnumerator VisualizeAStar(List<GameObject> path, float delay, Color color)
    {
        foreach (GameObject hex in path)
        {
            // Recolor cells that belongs to found path
            Renderer renderer = hex.GetComponent<Renderer>();
            if (renderer != null) renderer.material.color = color;
            yield return new WaitForSeconds(delay);
        }
        // Apply Dijkstra after dijkstraOffset steps of A*
        if (aStarPath.Count > 10)
        {
            List<GameObject> dijkstra = Dijkstra(aStarPath[dijkstraOffset-1], endHex);
            StartCoroutine(VisualizeDijkstra(dijkstra.ToArray()[1..].ToList(), 0.5f, Color.red));
            Debug.Log("Dijkstra cost:"+PathCost(dijkstra));
        }
        else
        {
            Debug.Log("No need for dijkstra");
        }
    }
    private IEnumerator VisualizeDijkstra(List<GameObject> path, float delay, Color color)
    {
        foreach (GameObject hex in path)
        {
            // Recolor cells that belongs to found path
            var renderer = hex.GetComponent<Renderer>();
            if (renderer) renderer.material.color = color;
            yield return new WaitForSeconds(delay);
        }
    }



    private List<GameObject> AStar(GameObject start, GameObject goal)
    {
        var currentPathCost = new Dictionary<GameObject, float>();
        var distanceToEnd = new Dictionary<GameObject, float>();
        // Path representation
        var cameFrom = new Dictionary<GameObject, GameObject>();

        foreach (var hex in hexGrid.hexes)
        {
            // Initially all values are infinities 
            currentPathCost[hex] = float.MaxValue;
            distanceToEnd[hex] = float.MaxValue;
        }

        currentPathCost[start] = 0f;
        distanceToEnd[start] = Heuristic(start, goal);

        // init the data structure to store all possible next steps
        var openSet = new SortedSet<GameObject>(Comparer<GameObject>.Create((c1, c2) =>
        {
            var distC1 = distanceToEnd.ContainsKey(c1) ? distanceToEnd[c1] : float.MaxValue;
            var distC2 = distanceToEnd.ContainsKey(c2) ? distanceToEnd[c2] : float.MaxValue;
            var comparison = distC1.CompareTo(distC2);
            return comparison == 0 ? c1.GetHashCode().CompareTo(c2.GetHashCode()) : comparison;
        }));

        
        openSet.Add(start);
        var numIters = 0;
        // main loop go until all paths found
        while (openSet.Count > 0)
        {
            numIters += 1;
            if (numIters > 100000)
            {
                // preventing infinite loops
                Debug.Log("Infinite loop A*");
                break;
            }

            var current = openSet.Min;
            openSet.Remove(current);
            
            // early stopping criteria
            if (current == goal) return ReconstructPath(cameFrom, current);

            foreach (var neighbor in GetNeighbors(current))
            {
                // go through all neighbours and calculate their metrics 
                float tentativeGScore = currentPathCost[current] + TransitionCost(current, neighbor);
                if (tentativeGScore < currentPathCost[neighbor])
                {
                    cameFrom[neighbor] = current;
                    currentPathCost[neighbor] = tentativeGScore;
                    distanceToEnd[neighbor] = currentPathCost[neighbor] + Heuristic(neighbor, goal);
                    openSet.Add(neighbor);
                }
            }
        }

        return null;
    }



    private float Heuristic(GameObject a, GameObject b)
    {
        // criteria to detect which cell is better
        return Vector2.Distance(
            new Vector2(a.transform.position.x, a.transform.position.z),
            new Vector2(b.transform.position.x, b.transform.position.z)
        );
    }

    private float TransitionCost(GameObject a, GameObject b)
    {
        // cost of transition from one cell to another
        return Mathf.Abs(a.transform.position.y - b.transform.position.y);
    }

    private List<GameObject> GetNeighbors(GameObject hex)
    {
        // return all neighbours
        if (hexNeighbors.ContainsKey(hex)) return hexNeighbors[hex];

        // Find neighbors knowing how indexes represents the grid 
        List<GameObject> neighbors = new List<GameObject>();
        var ind = hex.GetComponent<HexagonalPrismGenerator>().index;
        var w = hexGrid.gridWidth;
        var h = hexGrid.gridHeight;
        if (ind - 1 >= 0 && _isClose(hex, hexGrid.hexes[ind - 1])) neighbors.Add(hexGrid.hexes[ind - 1]);
        if (ind + 1 < w * h && _isClose(hex, hexGrid.hexes[ind + 1])) neighbors.Add(hexGrid.hexes[ind + 1]);
        if (ind - w >= 0 && _isClose(hex, hexGrid.hexes[ind - w])) neighbors.Add(hexGrid.hexes[ind - w]);
        if (ind + w < w * h && _isClose(hex, hexGrid.hexes[ind + w])) neighbors.Add(hexGrid.hexes[ind + w]);
        if (ind - w + 1 >= 0 && _isClose(hex, hexGrid.hexes[ind - w + 1])) neighbors.Add(hexGrid.hexes[ind - w + 1]);
        if (ind + w + 1 < w * h && _isClose(hex, hexGrid.hexes[ind + w + 1])) neighbors.Add(hexGrid.hexes[ind + w + 1]);

        hexNeighbors[hex] = neighbors;
        return neighbors;
    }

    private bool _isClose(GameObject h1, GameObject h2)
    {
        // detect whether to cells are adjacent
        return Heuristic(h1, h2) <= hexGrid.hexRadius * 2;
    }

    private List<GameObject> ReconstructPath(Dictionary<GameObject, GameObject> cameFrom, GameObject current)
    {
        // represent the path in correct order
        var path = new List<GameObject> { current };
        while (cameFrom.ContainsKey(current) && cameFrom[current] != null)
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        return path;
    }

    private List<GameObject> Dijkstra(GameObject start, GameObject goal)
    {
        // minimal cost from start cell each cell
        var distances = new Dictionary<GameObject, float>();
        // path representation
        var previous = new Dictionary<GameObject, GameObject>();
        // closed set of cells that were processed
        var visited = new HashSet<GameObject>();

        // initail values
        foreach (var hex in hexGrid.hexes)
        {
            distances[hex] = float.MaxValue;
            previous[hex] = null;
        }
        distances[start] = 0;
        
        // Main data structure to store next steps
        var priorityQueue = new SortedSet<GameObject>(Comparer<GameObject>.Create((c1, c2) =>
        {
            var res = distances[c1].CompareTo(distances[c2]);
            return res == 0 ? c1.GetHashCode().CompareTo(c2.GetHashCode()) : res;
        }));

        priorityQueue.Add(start);
        
        var numIters = 0;
        // main loop go until all paths found
        while (priorityQueue.Count > 0)
        {
            numIters += 1;
            if (numIters > 100000)
            {
                // preventing infinite loops
                Debug.Log("Infinite loop Dijkstra");
                break;
            }
            // take the cell with minimal distance from start cell
            var current = priorityQueue.Min;
            priorityQueue.Remove(current);

            // early stopping critaria
            if (current == goal) return ReconstructPath(previous, current);


            visited.Add(current);
            
            // go through not visited neighbours 
            foreach (var neighbor in GetNeighbors(current))
            {
                if (visited.Contains(neighbor)) continue;

                // update distances
                var tentativeDistance = distances[current] + TransitionCost(current, neighbor);
                if (tentativeDistance >= distances[neighbor]) continue;
                priorityQueue.Remove(neighbor);
                distances[neighbor] = tentativeDistance;
                previous[neighbor] = current;
                priorityQueue.Add(neighbor);
                
            }
        }

        Debug.LogWarning("Path not found.");
        return null;
    }
    private float PathCost(List<GameObject> path)
    {
        // calculate total cost of a path
        float totalCost = 0f;
        for (int i = 0; i < path.Count - 1; i++) totalCost += TransitionCost(path[i], path[i + 1]);
        return totalCost;
    }

}