using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator Instance { get; private set; }

    [Header("--- Map Dimensions ---")]
    public int mapX;
    public int mapY;
    public float noiseScale;

    [Header("--- Height Settings ---")]
    public float waterHeight;
    public float sandHeight;
    public float plainHeight;
    public float forestHeight;
    public float hillHeight;

    [Header("--- Prefabs ---")]
    public GameObject waterTilePrefab;
    public GameObject sandTilePrefab;
    public GameObject plainTilePrefab;
    public GameObject forestTilePrefab;
    public GameObject hillTilePrefab;
    public GameObject mountainTilePrefab;
    public GameObject borderTilePrefab;
    public GameObject core;

    [Header("--- Variant Arrays ---")]
    [SerializeField] GameObject[] plainArray;
    [SerializeField] GameObject[] forestArray;
    [SerializeField] GameObject[] hillArray;
    [SerializeField] int[] plainPercentArray;
    [SerializeField] int[] forestPercentArray;
    [SerializeField] int[] hillPercentArray;

    [Header("--- Edge Spawn Settings ---")]
    [SerializeField] int edgeDistance = 3;
    [SerializeField] float coreExcludeDistance = 20f;
    [SerializeField] bool debugShowSpawnTiles = true; // デバッグ用：スポーン地点を赤くする

    public List<GameObject> EdgeForestTiles = new List<GameObject>();
    public GameObject[,] Tiles;
    private List<GameObject> Borders = new List<GameObject>();
    private PlayerObject playerObject;
    public Node[,] grid;
    public Vector2 selectedPosition;

    void Awake()
    {
        // シングルトンの設定（最初に行う）
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Tiles = new GameObject[mapX, mapY];
        playerObject = FindObjectOfType<PlayerObject>();

        GenerateTiles();
        GenerateBorderObjects();
        RemoveTileCollider();
        SelectRandomPlainTile();
        FilterEdgeForestTilesByCore(); // コアが決まった後にフィルタリング
        InitializeGrid(mapX, mapY);
    }

    void GenerateTiles()
    {
        float offsetX = UnityEngine.Random.Range(1, 10000);
        float offsetY = UnityEngine.Random.Range(1, 10000);

        for (int x = 0; x < mapX; x++)
        {
            for (int y = 0; y < mapY; y++)
            {
                float height = Mathf.PerlinNoise((x * noiseScale) + offsetX, (y * noiseScale) + offsetY);
                Vector2 position = new Vector2(x, y);
                GameObject tile = null;

                if (height < waterHeight) tile = Instantiate(waterTilePrefab, position, Quaternion.identity, transform);
                else if (height < sandHeight) tile = Instantiate(sandTilePrefab, position, Quaternion.identity, transform);
                else if (height < plainHeight)
                {
                    int index = DetermineWeightedIndex(plainPercentArray);
                    tile = Instantiate(plainArray[index], position, Quaternion.identity, transform);
                }
                else if (height < forestHeight)
                {
                    int index = DetermineWeightedIndex(forestPercentArray);
                    tile = Instantiate(forestArray[index], position, Quaternion.identity, transform);

                    // 外縁判定
                    if (tile.layer != LayerMask.NameToLayer("Obstacle") && IsOnEdge(x, y, edgeDistance))
                    {
                        EdgeForestTiles.Add(tile);
                    }
                }
                else if (height < hillHeight)
                {
                    int index = DetermineWeightedIndex(hillPercentArray);
                    tile = Instantiate(hillArray[index], position, Quaternion.identity, transform);
                }
                else tile = Instantiate(mountainTilePrefab, position, Quaternion.identity, transform);

                Tiles[x, y] = tile;
            }
        }
    }

    bool IsOnEdge(int x, int y, int distance)
    {
        return x < distance || x >= mapX - distance || y < distance || y >= mapY - distance;
    }

    void FilterEdgeForestTilesByCore()
    {
        int obstacleLayer = LayerMask.NameToLayer("Obstacle");
        // コアからの距離でフィルタリング
        EdgeForestTiles = EdgeForestTiles
            .Where(tile => Vector2.Distance(tile.transform.position, core.transform.position) > coreExcludeDistance)
            .ToList();

        // デバッグ用：スポーン地点を赤く染める
        if (debugShowSpawnTiles)
        {
            foreach (GameObject tile in EdgeForestTiles)
            {
                var renderer = tile.GetComponent<SpriteRenderer>();
                if (renderer != null) renderer.color = Color.red;
                tile.layer = obstacleLayer;
                tile.AddComponent<BoxCollider2D>();
                BoxCollider2D col = tile.GetComponent<BoxCollider2D>();
                col.isTrigger = true;
            }
        }
    }

    void GenerateBorderObjects()
    {
        for (int y = -1; y <= mapY; y++)
        {
            Borders.Add(Instantiate(borderTilePrefab, new Vector2(-1, y), Quaternion.identity, transform));
            Borders.Add(Instantiate(borderTilePrefab, new Vector2(mapX, y), Quaternion.identity, transform));
        }
        for (int x = 0; x < mapX; x++)
        {
            Borders.Add(Instantiate(borderTilePrefab, new Vector2(x, -1), Quaternion.identity, transform));
            Borders.Add(Instantiate(borderTilePrefab, new Vector2(x, mapY), Quaternion.identity, transform));
        }
    }

    private void RemoveTileCollider()
    {
        foreach (GameObject tile in Tiles)
        {
            if (tile.tag != "mountain" && tile.tag != "water" && tile.layer != LayerMask.NameToLayer("Obstacle"))
            {
                BoxCollider2D col = tile.GetComponent<BoxCollider2D>();
                if (col != null) Destroy(col);
                Rigidbody2D rb = tile.GetComponent<Rigidbody2D>();
                if (rb != null) Destroy(rb);
            }
        }
    }

    private void SelectRandomPlainTile()
    {
        List<GameObject> plains = new List<GameObject>();
        foreach (var t in Tiles) if (t != null && t.CompareTag("plain")) plains.Add(t);

        if (plains.Count > 0)
        {
            GameObject target = plains[UnityEngine.Random.Range(0, plains.Count)];
            selectedPosition = target.transform.position;
            if (playerObject != null) playerObject.transform.position = selectedPosition;
            if (core != null) core.transform.position = selectedPosition;
        }
    }

    public void InitializeGrid(int width, int height)
    {
        grid = new Node[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int cost = 1;
                bool walkable = true;
                GameObject tile = Tiles[x, y];
                if (tile.CompareTag("border") || tile.CompareTag("mountain") || tile.CompareTag("hillObs")) walkable = false;
                else if (tile.CompareTag("water")) cost = 5;

                grid[x, y] = new Node(new Vector2Int(x, y), walkable, cost);
            }
        }
    }

    int DetermineWeightedIndex(int[] percentArray)
    {
        if (percentArray == null || percentArray.Length == 0) return 0;
        int total = percentArray.Sum();
        int r = UnityEngine.Random.Range(0, total);
        int current = 0;
        for (int i = 0; i < percentArray.Length; i++)
        {
            current += percentArray[i];
            if (r < current) return i;
        }
        return percentArray.Length - 1;
    }
}