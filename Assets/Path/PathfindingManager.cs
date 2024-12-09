using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class PathfindingManager : MonoBehaviour
{
    public HexGrid hexGrid; // Reference to your HexGrid script
    public GameObject startHex; // Starting hex cell
    public GameObject endHex; // Target hex cell
    private Dictionary<GameObject, List<GameObject>> hexNeighbors = new Dictionary<GameObject, List<GameObject>>();
    private List<GameObject> aStarPath;
    
    void Update()
    {
        if (hexGrid.isEndSelected)
        {
            startHex = HexagonSelector.startHex;
            endHex = HexagonSelector.endHex;
            StartCoroutine(DelayedPathfinding(5f));
        }

        hexGrid.isEndSelected = false;
    }

    private IEnumerator DelayedPathfinding(float delay)
    {
        // Ждем указанное количество секунд
        yield return new WaitForSeconds(delay);
        Debug.Log(hexGrid.hexes.Count);

        // Find the initial path using A*
        aStarPath  = AStar(startHex, endHex);

        
        if (aStarPath  != null)
        {
            StartCoroutine(VisualizeAStar(aStarPath.ToArray()[..10].ToList(), 0.5f, Color.magenta));
            Debug.Log("A* cost:"+PathCost(aStarPath));
        }
        else
        {
            Debug.Log("A* path not found");
            List<GameObject> refinedPath = Dijkstra(aStarPath [Mathf.Min(10,aStarPath .Count)], endHex);
            StartCoroutine(VisualizeDijkstra(refinedPath, 0.5f, Color.red));
            Debug.Log("Dijkstra cost:"+PathCost(refinedPath));
        }
    }

    

    private IEnumerator VisualizeAStar(List<GameObject> path, float delay, Color color)
    {
        foreach (GameObject hex in path)
        {
            // Изменить цвет текущей клетки
            Renderer renderer = hex.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color; // Подсветить клетку красным
            }

            // Подождать указанное время перед подсветкой следующей клетки
            yield return new WaitForSeconds(delay);
        }
        List<GameObject> refinedPath = Dijkstra(aStarPath [Mathf.Min(10,aStarPath .Count)], endHex);
        StartCoroutine(VisualizeDijkstra(refinedPath, 0.5f, Color.red));
        Debug.Log("Dijkstra cost:"+PathCost(refinedPath));

    }
    private IEnumerator VisualizeDijkstra(List<GameObject> path, float delay, Color color)
    {
        foreach (GameObject hex in path)
        {
            // Изменить цвет текущей клетки
            Renderer renderer = hex.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color; // Подсветить клетку красным
            }

            // Подождать указанное время перед подсветкой следующей клетки
            yield return new WaitForSeconds(delay);
        }
    }



    private List<GameObject> AStar(GameObject start, GameObject goal)
    {
        var gScore = new Dictionary<GameObject, float>();
        var fScore = new Dictionary<GameObject, float>();
        var cameFrom = new Dictionary<GameObject, GameObject>();

        foreach (var hex in hexGrid.hexes)
        {

            gScore[hex] = float.MaxValue;
            fScore[hex] = float.MaxValue;
        }

        gScore[start] = 0f;
        fScore[start] = Heuristic(start, goal);

        var openSet = new SortedSet<GameObject>(Comparer<GameObject>.Create((a, b) =>
        {
            float fA = fScore.ContainsKey(a) ? fScore[a] : float.MaxValue;
            float fB = fScore.ContainsKey(b) ? fScore[b] : float.MaxValue;
            int comparison = fA.CompareTo(fB);
            return comparison == 0 ? a.GetHashCode().CompareTo(b.GetHashCode()) : comparison;
        }));

        openSet.Add(start);
        var numIters = 0;
        while (openSet.Count > 0)
        {
            numIters += 1;
            if (numIters > 10000)
            {
                Debug.Log("Inginite loop A*");
                break;
            }

            var current = openSet.Min;
            openSet.Remove(current);
            if (current == goal)
                return ReconstructPath(cameFrom, current);

            foreach (var neighbor in GetNeighbors(current))
            {
                float tentativeGScore = gScore[current] + TransitionCost(current, neighbor);
                if (tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, goal);

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null; // No path found
    }



    private float Heuristic(GameObject a, GameObject b)
    {
        return Vector2.Distance(
            new Vector2(a.transform.position.x, a.transform.position.z),
            new Vector2(b.transform.position.x, b.transform.position.z)
        );
    }

    private float TransitionCost(GameObject a, GameObject b)
    {
        return Mathf.Abs(a.transform.position.y - b.transform.position.y);
    }

    private List<GameObject> GetNeighbors(GameObject hex)
    {
        if (hexNeighbors.ContainsKey(hex))
            return hexNeighbors[hex];

        // Find neighbors based on spatial proximity
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
        return Heuristic(h1, h2) <= hexGrid.hexRadius * 2;
    }

    private List<GameObject> ReconstructPath(Dictionary<GameObject, GameObject> cameFrom, GameObject current)
    {
        var path = new List<GameObject> { current };

        while (cameFrom.ContainsKey(current) && cameFrom[current] != null)
        {
            current = cameFrom[current];
            path.Insert(0, current); // Добавляем в начало пути
        }

        return path;
    }

    private List<GameObject> Dijkstra(GameObject start, GameObject goal)
    {
        if (start == null || goal == null)
        {
            Debug.LogError("Start or goal is null.");
            return null;
        }

        // Инициализация
        Dictionary<GameObject, float> distances = new Dictionary<GameObject, float>();
        Dictionary<GameObject, GameObject> previous = new Dictionary<GameObject, GameObject>();
        HashSet<GameObject> visited = new HashSet<GameObject>();

        foreach (var hex in hexGrid.hexes)
        {
            distances[hex] = float.MaxValue; // Изначально все расстояния бесконечны
            previous[hex] = null; // Нет предыдущих узлов
        }

        distances[start] = 0; // Расстояние до начальной клетки = 0
        var priorityQueue = new SortedSet<GameObject>(Comparer<GameObject>.Create((a, b) =>
        {
            int cmp = distances[a].CompareTo(distances[b]);
            return cmp == 0 ? a.GetHashCode().CompareTo(b.GetHashCode()) : cmp;
        }));

        priorityQueue.Add(start);

        // Основной цикл
        while (priorityQueue.Count > 0)
        {
            GameObject current = priorityQueue.Min;
            priorityQueue.Remove(current);

            if (current == goal)
            {
                // Построение пути
                return ReconstructPath(previous, current);
            }

            visited.Add(current);

            foreach (var neighbor in GetNeighbors(current))
            {
                if (visited.Contains(neighbor)) continue;

                float tentativeDistance = distances[current] + TransitionCost(current, neighbor);

                if (tentativeDistance < distances[neighbor])
                {
                    priorityQueue.Remove(neighbor); // Удаляем из очереди, чтобы обновить приоритет
                    distances[neighbor] = tentativeDistance;
                    previous[neighbor] = current; // Обновляем предыдущий узел
                    priorityQueue.Add(neighbor);
                }
            }
        }

        Debug.LogWarning("Path not found.");
        return null; // Путь не найден
    }
    private float PathCost(List<GameObject> path)
    {
        float totalCost = 0f;

        for (int i = 0; i < path.Count - 1; i++)
        {
            // Стоимость перехода между текущей и следующей клеткой
            totalCost += TransitionCost(path[i], path[i + 1]);
        }

        return totalCost;
    }

}