using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [System.Serializable]
    public class Variant { public GameObject prefab; public float weight = 1f; }

    public Variant[] wallVariants;     // intact, cracked, half-collapsed
    public Variant[] pillarVariants;   // standing, broken, toppled
    public Variant[] floorVariants;    // intact, cracked

    [Range(0f, 1f)] public float pillarChance = 0.7f;
    public bool mirror = true;         // symmetrical layout
    public int seed = 0;               // 0 = random each run
    public NavMeshSurface surface;
    int limitX;                        // carve only this many columns (half when mirrored)

    GameObject Pick(Variant[] list)
    {
        float total = 0f;
        foreach (var v in list) total += v.weight;
        float r = Random.value * total;
        foreach (var v in list) if ((r -= v.weight) <= 0f) return v.prefab;
        return list[0].prefab;
    }
    bool InCarveBounds(int x, int y) => x >= 0 && y >= 0 && x < limitX && y < height;

    public GameObject goalPrefab;
    public GameObject[] deadEndPrefabs;   // stairs, pickups, etc.

    public int width = 15;
    public int height = 15;
    public float cellSize = 6f;
    [Range(0f, 1f)] public float deadEndChance = 0.6f;
    [Range(0f, 0.3f)] public float braidChance = 0.08f;  // knocks out extra walls to make loops

    class Cell { public bool visited; public bool[] walls = { true, true, true, true }; } // N,E,S,W
    static readonly Vector2Int[] Dir = { new(0, 1), new(1, 0), new(0, -1), new(-1, 0) };

    Cell[,] grid;

    // Builds the maze and returns the world position of the start cell
    public Vector3 Generate(Vector3 origin)
    {
        if (seed != 0) Random.InitState(seed);
        if (mirror && width % 2 == 1) width++;       // mirroring needs an even width
        limitX = mirror ? width / 2 : width;

        grid = new Cell[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y] = new Cell();

        if (mirror) Mirror();
        SpawnFloors(origin);
        SpawnWalls(origin);
        SpawnPillars(origin);
        PlaceObjects(origin);

        return CellToWorld(origin, 0, 0);
    }

    Vector3 CellToWorld(Vector3 origin, int x, int y) =>
        origin + new Vector3(x * cellSize, 0, y * cellSize);


    void Carve()
    {
        var stack = new Stack<Vector2Int>();
        var start = new Vector2Int(0, 0);
        grid[start.x, start.y].visited = true;
        stack.Push(start);

        while (stack.Count > 0)
        {
            var cur = stack.Peek();
            var options = new List<int>();
            for (int d = 0; d < 4; d++)
            {
                int nx = cur.x + Dir[d].x, ny = cur.y + Dir[d].y;
                if (InCarveBounds(nx, ny) && !grid[nx, ny].visited) options.Add(d);
            }

            if (options.Count == 0) { stack.Pop(); continue; }

            int pick = options[Random.Range(0, options.Count)];
            var next = cur + Dir[pick];
            grid[cur.x, cur.y].walls[pick] = false;
            grid[next.x, next.y].walls[(pick + 2) % 4] = false;
            grid[next.x, next.y].visited = true;
            stack.Push(next);
        }
    }

    void Braid()
    {
        for (int x = 0; x < limitX; x++)
            for (int y = 0; y < height; y++)
                for (int d = 0; d < 4; d++)
                {
                    int nx = x + Dir[d].x, ny = y + Dir[d].y;
                    if (!InCarveBounds(nx, ny) || !grid[x, y].walls[d]) continue;
                    if (Random.value < braidChance)
                    {
                        grid[x, y].walls[d] = false;
                        grid[nx, ny].walls[(d + 2) % 4] = false;
                    }
                }
    }

    void SpawnWalls(Vector3 origin)
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                Vector3 c = CellToWorld(origin, x, y);
                var cell = grid[x, y];

                // Each shared wall is spawned once: N and E for every cell, S and W only on the border
                if (cell.walls[0]) SpawnWall(c + new Vector3(0, 0, cellSize / 2), 0);
                if (cell.walls[1]) SpawnWall(c + new Vector3(cellSize / 2, 0, 0), 90);
                if (y == 0 && cell.walls[2]) SpawnWall(c + new Vector3(0, 0, -cellSize / 2), 0);
                if (x == 0 && cell.walls[3]) SpawnWall(c + new Vector3(-cellSize / 2, 0, 0), 90);
            }
    }

    void PlaceObjects(Vector3 origin)
    {
        // BFS from the start to find distances
        var dist = new int[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++) dist[x, y] = -1;

        var q = new Queue<Vector2Int>();
        q.Enqueue(Vector2Int.zero);
        dist[0, 0] = 0;
        Vector2Int farthest = Vector2Int.zero;

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            if (dist[cur.x, cur.y] > dist[farthest.x, farthest.y]) farthest = cur;

            for (int d = 0; d < 4; d++)
            {
                if (grid[cur.x, cur.y].walls[d]) continue;
                var n = cur + Dir[d];
                if (!InCarveBounds(n.x, n.y) || dist[n.x, n.y] != -1) continue;
                dist[n.x, n.y] = dist[cur.x, cur.y] + 1;
                q.Enqueue(n);
            }
        }

        // Goal at the farthest cell
        if (goalPrefab != null)
            Instantiate(goalPrefab, CellToWorld(origin, farthest.x, farthest.y), Quaternion.identity, transform);

        // Dead ends (3 walls), excluding start and goal
        if (deadEndPrefabs == null || deadEndPrefabs.Length == 0) return;
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var p = new Vector2Int(x, y);
                if (p == Vector2Int.zero || p == farthest) continue;

                int wallCount = 0;
                foreach (bool w in grid[x, y].walls) if (w) wallCount++;
                if (wallCount < 3 || Random.value > deadEndChance) continue;

                var prefab = deadEndPrefabs[Random.Range(0, deadEndPrefabs.Length)];
                Instantiate(prefab, CellToWorld(origin, x, y), Quaternion.identity, transform);
            }
    }
    void Mirror()
    {
        for (int x = 0; x < limitX; x++)
            for (int y = 0; y < height; y++)
            {
                var s = grid[x, y];
                var m = grid[width - 1 - x, y];
                m.walls[0] = s.walls[0];   // N
                m.walls[2] = s.walls[2];   // S
                m.walls[1] = s.walls[3];   // E gets the original's W
                m.walls[3] = s.walls[1];   // W gets the original's E
            }

        // doors through the center line, so the two halves connect
        for (int y = 0; y < height; y++)
            if (y == height / 2 || y % 5 == 0)
            {
                grid[limitX - 1, y].walls[1] = false;
                grid[limitX, y].walls[3] = false;
            }
    }
    void SpawnWall(Vector3 pos, float yRot)
    {
        var prefab = Pick(wallVariants);
        var wall = Instantiate(prefab, pos, Quaternion.Euler(0, yRot, 0), transform);
        var s = prefab.transform.localScale;
        wall.transform.localScale = new Vector3(cellSize, s.y, s.z);
    }

    void SpawnFloors(Vector3 origin)
    {
        if (floorVariants == null || floorVariants.Length == 0) return;
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                Instantiate(Pick(floorVariants), CellToWorld(origin, x, y), Quaternion.identity, transform);
    }

    void SpawnPillars(Vector3 origin)
    {
        if (pillarVariants == null || pillarVariants.Length == 0) return;
        for (int x = 0; x <= width; x++)
            for (int y = 0; y <= height; y++)
            {
                if (Random.value > pillarChance) continue;
                Vector3 p = origin + new Vector3((x - 0.5f) * cellSize, 0, (y - 0.5f) * cellSize);
                Instantiate(Pick(pillarVariants), p, Quaternion.Euler(0, 90 * Random.Range(0, 4), 0), transform);
            }
    }
}