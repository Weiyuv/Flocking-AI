
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class AIFlokingGrid : MonoBehaviour
{
    public static AIFlokingGrid Instance { get; private set; }

    [Header("Grid Espacial")]
    [Min(0.1f)]
    public float cellSize = 2f;

    [Header("Visualizacao do Grid")]
    public bool showGrid = true;
    [Min(1)]
    public int visualRadius = 5;
    public float visualHeight = 0f;
    public Color gridColor = Color.cyan;

    private readonly Dictionary<Vector3Int, List<AIFloking>> grid =
        new Dictionary<Vector3Int, List<AIFloking>>();

    private readonly List<AIFloking> boids =
        new List<AIFloking>();

    private readonly HashSet<AIFloking> registeredBoids =
        new HashSet<AIFloking>();

    private readonly List<List<AIFloking>> listPool =
        new List<List<AIFloking>>();

    private int poolIndex;

    public int BoidCount => boids.Count;
    public int CellCount => grid.Count;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Ja existe outro AIFlokingGrid na cena.");
            enabled = false;
            return;
        }

        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Register(AIFloking boid)
    {
        if (boid == null || !registeredBoids.Add(boid))
            return;

        boids.Add(boid);
    }

    public void Unregister(AIFloking boid)
    {
        if (boid == null || !registeredBoids.Remove(boid))
            return;

        boids.Remove(boid);
    }

    void FixedUpdate()
    {
        RebuildGrid();
    }

    void RebuildGrid()
    {
        foreach (List<AIFloking> cell in grid.Values)
            cell.Clear();

        grid.Clear();
        poolIndex = 0;

        for (int i = boids.Count - 1; i >= 0; i--)
        {
            AIFloking boid = boids[i];

            if (boid == null || !boid.isActiveAndEnabled ||
                boid.Body == null)
            {
                if (boid != null)
                    registeredBoids.Remove(boid);

                boids.RemoveAt(i);
                continue;
            }

            Vector3Int cellPosition = WorldToCell(boid.Body.position);

            if (!grid.TryGetValue(
                cellPosition,
                out List<AIFloking> cell))
            {
                cell = GetPooledList();
                grid.Add(cellPosition, cell);
            }

            cell.Add(boid);
        }
    }

    List<AIFloking> GetPooledList()
    {
        if (poolIndex < listPool.Count)
        {
            List<AIFloking> cell = listPool[poolIndex++];
            cell.Clear();
            return cell;
        }

        List<AIFloking> newList = new List<AIFloking>(16);
        listPool.Add(newList);
        poolIndex++;

        return newList;
    }

    public Vector3Int WorldToCell(Vector3 position)
    {
        float size = Mathf.Max(0.1f, cellSize);

        return new Vector3Int(
            Mathf.FloorToInt(position.x / size),
            Mathf.FloorToInt(position.y / size),
            Mathf.FloorToInt(position.z / size)
        );
    }

    public List<AIFloking> GetCell(Vector3Int position)
    {
        grid.TryGetValue(position, out List<AIFloking> cell);
        return cell;
    }

    void OnDrawGizmos()
    {
        if (!showGrid)
            return;

        float size = Mathf.Max(0.1f, cellSize);

        Vector3 center = transform.position;

        Vector3Int centerCell = new Vector3Int(
            Mathf.FloorToInt(center.x / size),
            0,
            Mathf.FloorToInt(center.z / size)
        );

        float minX = (centerCell.x - visualRadius) * size;
        float maxX = (centerCell.x + visualRadius + 1) * size;
        float minZ = (centerCell.z - visualRadius) * size;
        float maxZ = (centerCell.z + visualRadius + 1) * size;

        Gizmos.color = gridColor;

        for (int x = -visualRadius; x <= visualRadius + 1; x++)
        {
            float worldX = (centerCell.x + x) * size;

            Gizmos.DrawLine(
                new Vector3(worldX, visualHeight, minZ),
                new Vector3(worldX, visualHeight, maxZ)
            );
        }

        for (int z = -visualRadius; z <= visualRadius + 1; z++)
        {
            float worldZ = (centerCell.z + z) * size;

            Gizmos.DrawLine(
                new Vector3(minX, visualHeight, worldZ),
                new Vector3(maxX, visualHeight, worldZ)
            );
        }
    }
}
