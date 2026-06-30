using System.Collections;
using UnityEngine;

public class SimpleTargetSeeker : MonoBehaviour, IPausable
{
    private Rigidbody2D rb;
    private Vector2 targetPosition;

    public float speed;
    public float waterMoveSpeed;
    public float mountainMoveSpeed;
    private float originalMoveSpeed;

    // 回転させるオブジェクト（nullの場合は自分自身を回転）
    [SerializeField] private Transform rotationTarget;

    // スロー効果用
    private Coroutine slowCoroutine;
    private float currentSlowPercent = 0f;

    [SerializeField] bool drawDebugLines;

    // ===== 一時停止機能 =====
    private bool isPaused = false;
    private Vector2 savedVelocity;
    private float savedAngularVelocity;

    void OnEnable()
    {
        PauseManager.Register(this);
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
            savedAngularVelocity = rb.angularVelocity;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;
        }
    }

    public void OnResume()
    {
        isPaused = false;
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = savedVelocity;
            rb.angularVelocity = savedAngularVelocity;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalMoveSpeed = speed;

        MapGenerator mapGenerator = FindObjectOfType<MapGenerator>();
        if (mapGenerator != null)
        {
            targetPosition = mapGenerator.selectedPosition;
        }
        else
        {
            Debug.LogError("MapGeneratorが見つかりません。目的地が設定できませんでした。");
            targetPosition = transform.position;
        }
    }

    void Update()
    {
        if (isPaused) return;

        if (drawDebugLines)
        {
            Debug.DrawLine((Vector2)targetPosition, (Vector2)transform.position, Color.red);
        }
    }

    void FixedUpdate()
    {
        if (isPaused) return;

        Vector2 direction = targetPosition - (Vector2)transform.position;

        if (direction.sqrMagnitude < 0.01f)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        rb.velocity = direction.normalized * speed;

        Vector2 normalizedMovement = rb.velocity.normalized;

        if (normalizedMovement != Vector2.zero) 
        {
            float angle = Mathf.Atan2(normalizedMovement.y, normalizedMovement.x) * Mathf.Rad2Deg;
            
            // 指定オブジェクトがあればそれを、なければ自分を回転
            if (rotationTarget != null)
            {
                rotationTarget.rotation = Quaternion.Euler(0, 0, angle - 90);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 0, angle - 90);
            }
        }
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

    /// <summary>
    /// 速度を低下させる
    /// </summary>
    /// <param name="slowPercent">低下率（%）例: 50 = 50%低下</param>
    /// <param name="duration">効果時間（秒）</param>
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

        float elapsed = 0f;

        // ===== 一時停止対応のWait =====
        while (elapsed < duration)
        {
            if (!isPaused)
            {
                elapsed += Time.deltaTime;
            }
            yield return null;
        }

        currentSlowPercent = 0f;
        speed = originalMoveSpeed;
        slowCoroutine = null;
    }
}