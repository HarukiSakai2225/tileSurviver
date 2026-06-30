using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToNearestObj : MonoBehaviour, IPausable
{
    [Header("探索設定")]
    [SerializeField] private string targetLayerName;          // ターゲットのレイヤー名
    [SerializeField] private float searchRadius = 20f;        // 探索範囲
    [SerializeField] private float searchInterval = 0.5f;     // 探索頻度（秒）

    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 5f;            // 移動速度
    [SerializeField] private float keepDistance = 2f;         // ターゲットとの距離を保つ
    [SerializeField] private bool rotateToTarget = true;      // ターゲット方向に回転するか
    [SerializeField] private Transform rotationTarget;        // 回転させるオブジェクト（nullなら自分）

    [Header("デバッグ")]
    [SerializeField] private bool drawDebugLines = false;

    private Rigidbody2D rb;
    private int targetLayerMask;
    private Transform currentTarget;
    private float searchTimer;

    // MapGeneratorから取得するデフォルトの目標地点
    private Vector2 defaultTargetPosition;
    private bool hasDefaultTarget = false;

    // ===== 一時停止機能 =====
    private bool isPaused = false;
    private Vector2 savedVelocity;
    private float savedAngularVelocity;

    void OnEnable()
    {
        // PauseManagerに自身を登録
        PauseManager.Register(this);
    }

    void OnDisable()
    {
        // PauseManagerから自身を解除
        PauseManager.Unregister(this);
    }

    // 一時停止時の処理
    public void OnPause()
    {
        isPaused = true;
        if (rb != null)
        {
            // 現在の速度を保存して停止させる
            savedVelocity = rb.velocity;
            savedAngularVelocity = rb.angularVelocity;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true; // 物理演算の影響を受けないようにする
        }
    }

    // 再開時の処理
    public void OnResume()
    {
        isPaused = false;
        if (rb != null)
        {
            rb.isKinematic = false;
            // 保存していた速度を復元する
            rb.velocity = savedVelocity;
            rb.angularVelocity = savedAngularVelocity;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        targetLayerMask = LayerMask.GetMask(targetLayerName);
        searchTimer = 0f;

        // MapGeneratorからデフォルトの目標地点を取得
        MapGenerator mapGenerator = FindObjectOfType<MapGenerator>();
        if (mapGenerator != null)
        {
            defaultTargetPosition = mapGenerator.selectedPosition;
            hasDefaultTarget = true;
        }
        else
        {
            Debug.LogWarning("MapGeneratorが見つかりません。デフォルトの目標地点が設定できませんでした。");
            defaultTargetPosition = transform.position;
            hasDefaultTarget = false;
        }

        // 初回探索
        SearchNearestTarget();
    }

    void Update()
    {
        // 一時停止中は処理を行わない
        if (isPaused) return;

        // 探索タイマー（一時停止中はカウントが進まない）
        searchTimer += Time.deltaTime;
        if (searchTimer >= searchInterval)
        {
            SearchNearestTarget();
            searchTimer = 0f;
        }

        // デバッグ表示
        if (drawDebugLines)
        {
            if (currentTarget != null)
            {
                Debug.DrawLine(transform.position, currentTarget.position, Color.green);
            }
            else if (hasDefaultTarget)
            {
                Debug.DrawLine(transform.position, defaultTargetPosition, Color.red);
            }
        }
    }

    void FixedUpdate()
    {
        // 一時停止中は移動計算を行わない
        if (isPaused) return;

        Vector2 targetPosition;
        float stopDistance;

        // ターゲットがいる場合
        if (currentTarget != null)
        {
            targetPosition = currentTarget.position;
            stopDistance = keepDistance;
        }
        // ターゲットがいない場合はデフォルト目標に向かう
        else if (hasDefaultTarget)
        {
            targetPosition = defaultTargetPosition;
            stopDistance = 0.1f; 
        }
        else
        {
            rb.velocity = Vector2.zero;
            return;
        }

        Vector2 direction = targetPosition - (Vector2)transform.position;
        float distance = direction.magnitude;

        // 指定距離より近い場合は停止、遠い場合は移動
        if (distance <= stopDistance)
        {
            rb.velocity = Vector2.zero;
        }
        else
        {
            rb.velocity = direction.normalized * moveSpeed;
        }

        // 回転処理
        if (rotateToTarget && direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle - 90);

            if (rotationTarget != null)
            {
                rotationTarget.rotation = targetRotation;
            }
            else
            {
                transform.rotation = targetRotation;
            }
        }
    }

    void SearchNearestTarget()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, searchRadius, targetLayerMask);

        float nearestDistance = float.MaxValue;
        Transform nearestTarget = null;

        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.gameObject == this.gameObject) continue;

            float distance = Vector2.Distance(transform.position, hitCollider.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestTarget = hitCollider.transform;
            }
        }

        currentTarget = nearestTarget;
    }

    // --- 各種Publicメソッド ---

    public Transform GetCurrentTarget() => currentTarget;
    public bool HasTarget() => currentTarget != null;
    public bool IsMovingToDefaultTarget() => currentTarget == null && hasDefaultTarget;
    
    public float GetDistanceToTarget()
    {
        if (currentTarget != null) return Vector2.Distance(transform.position, currentTarget.position);
        if (hasDefaultTarget) return Vector2.Distance(transform.position, defaultTargetPosition);
        return -1f;
    }

    public void SetSearchRadius(float radius) => searchRadius = radius;
    public void SetMoveSpeed(float speed) => moveSpeed = speed;
    public void SetKeepDistance(float distance) => keepDistance = distance;
    public void SetDefaultTargetPosition(Vector2 position)
    {
        defaultTargetPosition = position;
        hasDefaultTarget = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, searchRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, keepDistance);

        if (hasDefaultTarget)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(defaultTargetPosition, 0.5f);
        }
    }
}