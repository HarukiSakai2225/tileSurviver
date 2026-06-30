using System.Collections;
using UnityEngine;

public class SSSimpleTargetSeeker : MonoBehaviour, IPausable
{
    private Rigidbody2D rb;

    private Vector2 targetPosition;
    private bool hasTarget = false;

    public float speed;
    public float waterMoveSpeed;
    public float mountainMoveSpeed;

    private float originalMoveSpeed;

    [Header("Rotation")]
    [SerializeField] private Transform rotationTarget;

    [Header("Debug")]
    [SerializeField] private bool drawDebugLines;

    // スロー効果用
    private Coroutine slowCoroutine;
    private float currentSlowPercent = 0f;

    // 一時停止機能
    private bool isPaused = false;
    private Vector2 savedVelocity;
    private float savedAngularVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalMoveSpeed = speed;
    }

    private void OnEnable()
    {
        PauseManager.Register(this);
    }

    private void OnDisable()
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

    /// <summary>
    /// 外部から目的地を設定する
    /// </summary>
    public void SetTargetPosition(Vector2 position)
    {
        targetPosition = position;
        hasTarget = true;

        Debug.Log($"{gameObject.name} の目的地を設定しました: {targetPosition}");
    }

    private void Update()
    {
        if (isPaused) return;

        if (drawDebugLines && hasTarget)
        {
            Debug.DrawLine(transform.position, targetPosition, Color.red);
        }
    }

    private void FixedUpdate()
    {
        if (isPaused) return;
        if (!hasTarget) return;
        if (rb == null) return;

        Vector2 direction = targetPosition - (Vector2)transform.position;

        if (direction.sqrMagnitude < 0.01f)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        rb.velocity = direction.normalized * speed;

        RotateToMoveDirection(rb.velocity.normalized);
    }

    private void RotateToMoveDirection(Vector2 direction)
    {
        if (direction == Vector2.zero) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (rotationTarget != null)
        {
            rotationTarget.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
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

    private void OnTriggerExit2D(Collider2D other)
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

        float elapsed = 0f;

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