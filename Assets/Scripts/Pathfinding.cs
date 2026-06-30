using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour, IPausable
{
    MapGenerator mapGenerator;
    Node[,] grid;

    private List<Node> openList = new List<Node>();
    private HashSet<Node> closedList = new HashSet<Node>();
    private List<Node> neighborsList = new List<Node>();
    private List<Node> retracePathList = new List<Node>();
    private HashSet<Node> openListSet = new HashSet<Node>();

    [SerializeField] bool drawDebugLines;
    Rigidbody2D rb;
    [SerializeField] float FindAllPathAgainCircleTime;
    Vector2 targetPosition;
    List<Node> path = new List<Node>();
    float FindPathAgainTimer = 0;
    public float speed;
    public float waterMoveSpeed;
    public float mountainMoveSpeed;
    private float originalMoveSpeed;

    private Coroutine slowCoroutine;
    private float currentSlowPercent = 0f;

    int X;
    int Y;

    // 10秒以上2マス移動できなかったらDestroyするための設定
    [SerializeField] private float stuckDestroySeconds = 10f;
    [SerializeField] private float stuckDestroyRequiredMoveTiles = 2f;

    // 目的地点からこの距離以内なら、詰まりDestroy判定をしない
    [SerializeField] private float stuckDestroyTargetIgnoreDistanceTiles = 10f;

    private float stuckTimer = 0f;
    private float stuckMovedDistance = 0f;
    private Vector2 previousStuckCheckPosition;

    // 一時停止用
    private bool isPaused = false;
    private Vector2 savedVelocity;

    void OnEnable()
    {
        PauseManager.Register(this);

        if (PauseManager.IsPaused)
        {
            isPaused = true;
        }
    }

    void OnDisable()
    {
        PauseManager.Unregister(this);
    }

    public void OnPause()
    {
        isPaused = true;

        if (rb != null)
        {
            savedVelocity = rb.velocity;
            rb.velocity = Vector2.zero;
        }
    }

    public void OnResume()
    {
        isPaused = false;

        if (rb != null)
        {
            rb.velocity = savedVelocity;
        }

        ResetStuckCheck();
    }

    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        X = mapGenerator.mapX;
        Y = mapGenerator.mapY;
        rb = GetComponent<Rigidbody2D>();

        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        originalMoveSpeed = speed;
        targetPosition = mapGenerator.selectedPosition;
        path = FindPath(new Vector2Int((int)transform.position.x, (int)transform.position.y), new Vector2Int((int)targetPosition.x, (int)targetPosition.y));

        ResetStuckCheck();
    }

    void Update()
    {
        if (isPaused) return;

        FindPathAgainTimer += Time.deltaTime;

        if (FindPathAgainTimer > FindAllPathAgainCircleTime)
        {
            Vector2Int startPos = new Vector2Int(
                Mathf.RoundToInt(transform.position.x),
                Mathf.RoundToInt(transform.position.y)
            );
            startPos.x = Mathf.Clamp(startPos.x, 0, X - 1);
            startPos.y = Mathf.Clamp(startPos.y, 0, Y - 1);

            path = FindPath(startPos, new Vector2Int((int)targetPosition.x, (int)targetPosition.y));
            FindPathAgainTimer = 0;
        }

        if (drawDebugLines && path != null && path.Count > 1)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Debug.DrawLine((Vector2)path[i].GridPosition, (Vector2)path[i + 1].GridPosition, Color.red);
            }
        }

        Vector2 normalizedMovement = rb.velocity.normalized;

        if (normalizedMovement != Vector2.zero)
        {
            float angle = Mathf.Atan2(normalizedMovement.y, normalizedMovement.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90);
        }
    }

    void FixedUpdate()
    {
        if (isPaused) return;

        if (path != null)
        {
            if (path.Count > 0)
            {
                Vector2 dir = path[0].GridPosition - (Vector2)transform.position;
                rb.velocity = dir.normalized * speed;

                if (dir.sqrMagnitude < speed * Time.fixedDeltaTime * speed * Time.fixedDeltaTime)
                {
                    transform.position = (Vector2)path[0].GridPosition;
                    path.RemoveAt(0);
                    rb.velocity = Vector2.zero;

                    if (path.Count == 0)
                    {
                        path.Clear();
                    }
                }
            }
            else
            {
                rb.velocity = Vector2.zero;
            }
        }
        else
        {
            DestroySelf();
            return;
        }

        CheckStuckDestroy();
    }

    private void CheckStuckDestroy()
    {
        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);

        // 目的地点から10マス以内なら、詰まりDestroy判定をしない
        if (distanceToTarget <= stuckDestroyTargetIgnoreDistanceTiles)
        {
            ResetStuckCheck();
            return;
        }

        Vector2 currentPosition = transform.position;

        stuckMovedDistance += Vector2.Distance(previousStuckCheckPosition, currentPosition);
        previousStuckCheckPosition = currentPosition;

        if (stuckMovedDistance >= stuckDestroyRequiredMoveTiles)
        {
            stuckTimer = 0f;
            stuckMovedDistance = 0f;
            return;
        }

        stuckTimer += Time.fixedDeltaTime;

        if (stuckTimer >= stuckDestroySeconds)
        {
            DestroySelf();
        }
    }

    private void ResetStuckCheck()
    {
        stuckTimer = 0f;
        stuckMovedDistance = 0f;
        previousStuckCheckPosition = transform.position;
    }

    private void DestroySelf()
    {
        enemySpawnManager.Instance.decreaseSpawnedCount();
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("water"))
        {
            speed = waterMoveSpeed * (1f - currentSlowPercent / 100f);
        }
        else if (other.gameObject.CompareTag("mountain"))
        {
            speed = mountainMoveSpeed * (1f - currentSlowPercent / 100f);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("water") || other.gameObject.CompareTag("mountain"))
        {
            speed = originalMoveSpeed * (1f - currentSlowPercent / 100f);
        }
    }

    public void ApplySlow(float slowPercent, float duration)
    {
        if (slowPercent > currentSlowPercent)
        {
            if (slowCoroutine != null)
            {
                StopCoroutine(slowCoroutine);
            }

            slowCoroutine = StartCoroutine(SlowCoroutine(slowPercent, duration));
        }
    }

    private IEnumerator SlowCoroutine(float slowPercent, float duration)
    {
        currentSlowPercent = slowPercent;
        speed = originalMoveSpeed * (1f - slowPercent / 100f);

        yield return new WaitForSeconds(duration);

        currentSlowPercent = 0f;
        speed = originalMoveSpeed;
        slowCoroutine = null;
    }

    public List<Node> FindPath(Vector2Int start, Vector2Int target)
    {
        grid = mapGenerator.grid;

        if (grid == null || grid.Length == 0)
        {
            return null;
        }

        for (int x = 0; x < X; x++)
        {
            for (int y = 0; y < Y; y++)
            {
                if (grid[x, y] != null)
                {
                    grid[x, y].CCost = int.MaxValue;
                    grid[x, y].SCost = int.MaxValue;
                    grid[x, y].Parent = null;
                }
            }
        }

        if (start.x < 0 || start.x >= X || start.y < 0 || start.y >= Y ||
            target.x < 0 || target.x >= X || target.y < 0 || target.y >= Y)
        {
            Debug.LogError("StartまたはTargetがグリッド範囲外です。");
            return null;
        }

        Node startNode = grid[start.x, start.y];
        Node targetNode = grid[target.x, target.y];

        if (startNode == null || !startNode.IsWalkable || targetNode == null || !targetNode.IsWalkable)
        {
            return null;
        }

        openList.Clear();
        closedList.Clear();
        openListSet.Clear();

        startNode.CCost = 0;
        startNode.HCost = GetDistance(startNode, targetNode);
        startNode.SCost = startNode.HCost + startNode.CCost;
        startNode.Parent = null;

        openList.Add(startNode);
        openListSet.Add(startNode);

        Node currentNode;

        while (openList.Count > 0)
        {
            currentNode = binaryPop(openList);
            openListSet.Remove(currentNode);

            if (closedList.Contains(currentNode))
            {
                continue;
            }

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            closedList.Add(currentNode);

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (!neighbor.IsWalkable || closedList.Contains(neighbor))
                {
                    continue;
                }

                int newMovementCostToNeighbor = currentNode.CCost + neighbor.PCost;
                bool isInOpenList = openListSet.Contains(neighbor);

                if (newMovementCostToNeighbor < neighbor.CCost || !isInOpenList)
                {
                    neighbor.CCost = newMovementCostToNeighbor;
                    neighbor.HCost = GetDistance(neighbor, targetNode);
                    neighbor.SCost = neighbor.HCost + neighbor.CCost;
                    neighbor.Parent = currentNode;

                    if (!isInOpenList)
                    {
                        binaryAdd(neighbor, openList);
                        openListSet.Add(neighbor);
                    }
                    else
                    {
                        binaryAdd(neighbor, openList);
                    }
                }
            }
        }

        return null;
    }

    private List<Node> GetNeighbors(Node node)
    {
        neighborsList.Clear();

        Vector2Int[] directions = {
            new Vector2Int(0, 1),
            new Vector2Int(1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
        };

        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighborPos = node.GridPosition + direction;

            if (neighborPos.x >= 0 && neighborPos.x < grid.GetLength(0) && neighborPos.y >= 0 && neighborPos.y < grid.GetLength(1))
            {
                if (grid[neighborPos.x, neighborPos.y] != null)
                {
                    neighborsList.Add(grid[neighborPos.x, neighborPos.y]);
                }
            }
        }

        return neighborsList;
    }

    private int GetDistance(Node a, Node b)
    {
        int dstX = Mathf.Abs(a.GridPosition.x - b.GridPosition.x);
        int dstY = Mathf.Abs(a.GridPosition.y - b.GridPosition.y);
        return dstX + dstY;
    }

    private List<Node> RetracePath(Node startNode, Node endNode)
    {
        retracePathList.Clear();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            if (currentNode == null)
            {
                Debug.LogError("RetracePath failed: currentNode became null.");
                return new List<Node>();
            }

            retracePathList.Add(currentNode);
            currentNode = currentNode.Parent;
        }

        retracePathList.Reverse();

        return new List<Node>(retracePathList);
    }

    Node binaryPop(List<Node> list)
    {
        int listCount = list.Count;

        if (listCount == 0)
        {
            return null;
        }

        Node returnEle = list[0];
        list[0] = list[listCount - 1];
        list.RemoveAt(listCount - 1);
        listCount = list.Count;

        if (listCount <= 1)
        {
            return returnEle;
        }

        int currentIndex = 0;

        while (true)
        {
            int leftChildIndex = currentIndex * 2 + 1;
            int rightChildIndex = currentIndex * 2 + 2;
            int smallestChildIndex = currentIndex;

            if (leftChildIndex < listCount && list[leftChildIndex].SCost < list[smallestChildIndex].SCost)
            {
                smallestChildIndex = leftChildIndex;
            }

            if (rightChildIndex < listCount && list[rightChildIndex].SCost < list[smallestChildIndex].SCost)
            {
                smallestChildIndex = rightChildIndex;
            }

            if (smallestChildIndex == currentIndex)
            {
                break;
            }

            swap(list, currentIndex, smallestChildIndex);
            currentIndex = smallestChildIndex;
        }

        return returnEle;
    }

    void binaryAdd(Node ele, List<Node> list)
    {
        list.Add(ele);
        int currentIndex = list.Count - 1;

        while (currentIndex > 0)
        {
            int parentIndex = (currentIndex - 1) / 2;

            if (list[currentIndex].SCost < list[parentIndex].SCost)
            {
                swap(list, currentIndex, parentIndex);
                currentIndex = parentIndex;
            }
            else
            {
                break;
            }
        }
    }

    void swap(List<Node> list, int indexA, int indexB)
    {
        Node tmp = list[indexA];
        list[indexA] = list[indexB];
        list[indexB] = tmp;
    }
}