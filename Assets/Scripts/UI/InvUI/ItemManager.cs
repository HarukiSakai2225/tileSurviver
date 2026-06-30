using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemManager : MonoBehaviour
{
    [SerializeField] GameObject[] previewOBJ;
    ItemButton itemButtonScript;
    public int _indexNum = -1;
    int ind = -1;

    [SerializeField] int[] playerTilePCost;

    public List<GameObject> gunTurretList = new List<GameObject>();
    public List<GameObject> barrierList = new List<GameObject>();
    public List<GameObject> burstTurretList = new List<GameObject>();
    public List<GameObject> missileTurretList = new List<GameObject>();
    public List<GameObject> EMPLaserTurretList = new List<GameObject>();
    public List<GameObject> diggerList = new List<GameObject>();

    public Vector2[] previewSize;

    MapGenerator mapGenerator;

    private SpriteRenderer currentPreviewRenderer;
    private Color originalPreviewColor;
    private bool canPlace = false;

    [SerializeField] GameObject[] MasterObjArray;

    [SerializeField] int[] CostArray;

    [SerializeField] GameObject ExParent;
    [SerializeField] GameObject[] ExArray;

    // ★追加: ライン設置用の変数
    private bool isDrawingLine = false;
    private Vector2Int lineStartPos;
    private List<GameObject> linePreviewObjects = new List<GameObject>();
    [SerializeField] GameObject linePreviewPrefab; // ライン用プレビュー（なければpreviewOBJを使用）

    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
    }

    void Update()
    {
        int layerMask = LayerMask.GetMask("ItemUI");
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPosition, Vector2.zero, Mathf.Infinity, layerMask);

        // UIボタンのクリック判定
        if (hit.collider != null)
        {
            itemButtonScript = hit.collider.GetComponent<ItemButton>();
            if (Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftShift))
            {
                if (_indexNum != -1 && _indexNum != itemButtonScript.indexNum)
                {
                    if (currentPreviewRenderer != null)
                    {
                        currentPreviewRenderer.color = originalPreviewColor;
                    }
                    previewOBJ[_indexNum].SetActive(false);
                }

                _indexNum = itemButtonScript.indexNum;
                previewOBJ[_indexNum].SetActive(true);

                currentPreviewRenderer = previewOBJ[_indexNum].GetComponent<SpriteRenderer>();
                if (currentPreviewRenderer != null)
                {
                    originalPreviewColor = currentPreviewRenderer.color;
                }
            }
        }

        if (_indexNum != -1)
        {
            ExParent.SetActive(true);
            if (ind != _indexNum)
            {
                foreach (var item in ExArray)
                {
                    item.SetActive(false);
                }
                ExArray[_indexNum].SetActive(true);
                ind = _indexNum;
            }
        }
        else
        {
            ExParent.SetActive(false);
        }

        // アイテム選択解除（右クリック）
        if (_indexNum != -1 && Input.GetMouseButtonDown(1))
        {
            if (currentPreviewRenderer != null)
            {
                currentPreviewRenderer.color = originalPreviewColor;
            }

            previewOBJ[_indexNum].SetActive(false);
            _indexNum = -1;
            currentPreviewRenderer = null;
            canPlace = false;

            // ★追加: ライン描画もキャンセル
            CancelLineDraw();
        }

        // アイテム選択中の処理
        if (_indexNum != -1)
        {
            Vector2 mousePosition = Input.mousePosition;
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(worldPosition.x), Mathf.RoundToInt(worldPosition.y));

            // ★追加: Shift+左クリックでライン設置モード
            if (Input.GetKey(KeyCode.LeftShift))
            {
                HandleLineDrawing(worldPosition, gridPos);
            }
            else
            {
                // 通常の設置処理
                HandleNormalPlacement(worldPosition, gridPos);
            }
        }
    }

    // ★追加: _indexNum に応じた設置判定用レイヤーマスクを取得
    int GetPlacementCheckLayerMask()
    {
        // _indexNum が 0〜2 の場合は Water を判定対象に含めない
        if (_indexNum >= 0 && _indexNum <= 2)
        {
            return LayerMask.GetMask(
                "Obstacle",
                "Player",
                "playerObj",
                "UI",
                "ItemUI"
            );
        }

        // デフォルト条件は Water を含める
        return LayerMask.GetMask(
            "Obstacle",
            "Player",
            "playerObj",
            "UI",
            "ItemUI",
            "Water"
        );
    }

    // ★追加: ライン描画処理
    void HandleLineDrawing(Vector2 worldPosition, Vector2Int gridPos)
    {
        // 通常プレビューを非表示
        previewOBJ[_indexNum].SetActive(false);

        // ライン描画開始
        if (Input.GetMouseButtonDown(0))
        {
            int UILayerMask = LayerMask.GetMask("ItemUI", "MenuUI", "UI");
            RaycastHit2D UIhit = Physics2D.Raycast(worldPosition, Vector2.zero, Mathf.Infinity, UILayerMask);

            if (UIhit.collider == null)
            {
                isDrawingLine = true;
                lineStartPos = gridPos;
            }
        }

        // ライン描画中
        if (isDrawingLine && Input.GetMouseButton(0))
        {
            UpdateLinePreview(lineStartPos, gridPos);
        }

        // ライン描画終了（左クリック離す）
        if (isDrawingLine && Input.GetMouseButtonUp(0))
        {
            PlaceObjectsAlongLine(lineStartPos, gridPos);
            ClearLinePreview();
            isDrawingLine = false;
        }
    }

    // ★追加: 通常の設置処理（既存のコードを移動）
    void HandleNormalPlacement(Vector2 worldPosition, Vector2Int gridPos)
    {
        // ライン描画中なら何もしない
        if (isDrawingLine)
        {
            CancelLineDraw();
        }

        previewOBJ[_indexNum].SetActive(true);
        previewOBJ[_indexNum].transform.position = worldPosition;

        int placementCheckLayerMask = GetPlacementCheckLayerMask();
        RaycastHit2D placementHit = Physics2D.Raycast(
            worldPosition,
            Vector2.zero,
            Mathf.Infinity,
            placementCheckLayerMask
        );

        if (placementHit.collider == null && LvUpManager.Instance.CanConsumeTotalCoins(CostArray[_indexNum]))
        {
            canPlace = true;
            if (currentPreviewRenderer != null)
            {
                currentPreviewRenderer.color = originalPreviewColor;
            }
        }
        else
        {
            canPlace = false;
            if (currentPreviewRenderer != null)
            {
                currentPreviewRenderer.color = Color.red;
            }
        }

        // アイテム設置（左クリック）
        if (Input.GetMouseButtonDown(0))
        {
            int UILayerMask = GetPlacementCheckLayerMask();
            Vector2 TileSetmouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D TileSethit = Physics2D.Raycast(
                TileSetmouseWorldPosition,
                Vector2.zero,
                Mathf.Infinity,
                UILayerMask
            );

            if (TileSethit.collider == null && canPlace && LvUpManager.Instance.CanConsumeTotalCoins(CostArray[_indexNum]))
            {
                PlaceObject(new Vector2(gridPos.x, gridPos.y));
            }
        }
    }

    // ★追加: ラインプレビューを更新
    void UpdateLinePreview(Vector2Int start, Vector2Int end)
    {
        ClearLinePreview();

        List<Vector2Int> linePositions = GetLinePositions(start, end);
        int totalCost = linePositions.Count * CostArray[_indexNum];
        bool canAfford = LvUpManager.Instance.CanConsumeTotalCoins(totalCost);

        foreach (Vector2Int pos in linePositions)
        {
            GameObject preview = Instantiate(previewOBJ[_indexNum], new Vector2(pos.x, pos.y), Quaternion.identity);
            preview.SetActive(true);

            SpriteRenderer sr = preview.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                bool canPlaceHere = CanPlaceAtPosition(pos);
                if (canPlaceHere && canAfford)
                {
                    sr.color = new Color(0.580f, 1.000f, 0.580f, 0.808f); // 緑（半透明）
                }
                else
                {
                    sr.color = new Color(1f, 0f, 0f, 0.5f); // 赤（半透明）
                }
            }

            linePreviewObjects.Add(preview);
        }
    }

    // ★追加: ラインプレビューをクリア
    void ClearLinePreview()
    {
        foreach (GameObject obj in linePreviewObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        linePreviewObjects.Clear();
    }

    // ★追加: ライン描画をキャンセル
    void CancelLineDraw()
    {
        ClearLinePreview();
        isDrawingLine = false;
    }

    // ★追加: ライン上にオブジェクトを設置
    void PlaceObjectsAlongLine(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> linePositions = GetLinePositions(start, end);
        int totalCost = 0;

        // 設置可能な位置をカウント
        List<Vector2Int> placeablePositions = new List<Vector2Int>();
        foreach (Vector2Int pos in linePositions)
        {
            if (CanPlaceAtPosition(pos))
            {
                placeablePositions.Add(pos);
                totalCost += CostArray[_indexNum];
            }
        }

        // コストが足りるか確認
        if (!LvUpManager.Instance.CanConsumeTotalCoins(totalCost))
        {
            Debug.Log("コストが足りません");
            return;
        }

        // 設置
        foreach (Vector2Int pos in placeablePositions)
        {
            if (LvUpManager.Instance.CanConsumeTotalCoins(CostArray[_indexNum]))
            {
                PlaceObject(new Vector2(pos.x, pos.y));
            }
        }
    }

    // ★追加: 指定位置に設置可能かチェック
    bool CanPlaceAtPosition(Vector2Int pos)
    {
        int placementCheckLayerMask = GetPlacementCheckLayerMask();
        RaycastHit2D hit = Physics2D.Raycast(
            new Vector2(pos.x, pos.y),
            Vector2.zero,
            Mathf.Infinity,
            placementCheckLayerMask
        );

        return hit.collider == null;
    }

    // ★追加: Bresenhamのラインアルゴリズムで座標を取得
    List<Vector2Int> GetLinePositions(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> positions = new List<Vector2Int>();

        int x0 = start.x;
        int y0 = start.y;
        int x1 = end.x;
        int y1 = end.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            positions.Add(new Vector2Int(x0, y0));

            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }

        return positions;
    }

    // ★追加: オブジェクト設置処理（共通化）
    void PlaceObject(Vector2 gridPosition)
    {
        if (!LvUpManager.Instance.TryConsumeTotalCoins(CostArray[_indexNum]))
        {
            return;
        }

        // オブジェクトを生成
        GameObject placedTile = Instantiate(MasterObjArray[_indexNum], gridPosition, Quaternion.identity);

        // IPausableを持っていてポーズ中なら OnPause() を呼び出す
        IPausable pausable = placedTile.GetComponent<IPausable>();
        if (pausable != null && PauseManager.IsPaused)
        {
            pausable.OnPause();
        }

        placedTile.layer = LayerMask.NameToLayer("playerObj");

        SpriteRenderer sr = placedTile.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 1;
            sr.color = originalPreviewColor;
        }

        // リスト管理
        if (_indexNum == 0 || _indexNum == 1 || _indexNum == 2)
        {
            barrierList.Add(placedTile);
        }
        else if (_indexNum == 3)
        {
            gunTurretList.Add(placedTile);
        }
        else if (_indexNum == 4)
        {
            burstTurretList.Add(placedTile);
        }
        else if (_indexNum == 5)
        {
            missileTurretList.Add(placedTile);
        }
        else if (_indexNum == 6)
        {
            EMPLaserTurretList.Add(placedTile);
        }
        else if (_indexNum == 7)
        {
            diggerList.Add(placedTile);
        }

        LvUpManager.Instance.SetItemLists(
            gunTurretList,
            burstTurretList,
            missileTurretList,
            EMPLaserTurretList,
            barrierList,
            diggerList
        );

        placedTile.transform.localScale = previewSize[_indexNum];

        // グリッド座標を整数で取得
        int gridX = (int)gridPosition.x;
        int gridY = (int)gridPosition.y;
        Vector2Int gridPosInt = new Vector2Int(gridX, gridY);

        // HPManager を取得
        HPManager hpManager = placedTile.GetComponent<HPManager>();

        if (mapGenerator != null && hpManager != null)
        {
            int originalCost = mapGenerator.grid[gridX, gridY].CCost;
            mapGenerator.grid[gridX, gridY].PCost = playerTilePCost[_indexNum];
            hpManager.SetTile(gridPosInt, originalCost);
        }
        else if (mapGenerator != null)
        {
            mapGenerator.grid[gridX, gridY].CCost = playerTilePCost[_indexNum];
        }

        Rigidbody2D rb = placedTile.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        BoxCollider2D col = placedTile.GetComponent<BoxCollider2D>();
        col.enabled = true;
    }
}