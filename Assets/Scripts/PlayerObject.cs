using UnityEngine;

public class PlayerObject : MonoBehaviour, IPausable
{
    public float moveSpeed;
    public float waterMoveSpeed;
    public float mountainMoveSpeed;
    private float originalMoveSpeed;
    private Rigidbody2D rb;

    public Transform spriteTransform;

    public float acceleration = 50f;
    public float deceleration = 100f;

    [SerializeField] bool isPlayer;

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
        // プレイヤーは入力で動くので速度復元は不要
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalMoveSpeed = moveSpeed;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
    }

    void FixedUpdate()
    {
        if (isPaused) return;

        Vector2 movement = Vector2.zero;

        if (Input.GetKey(KeyCode.W)) { movement += Vector2.up; }
        if (Input.GetKey(KeyCode.S)) { movement += Vector2.down; }
        if (Input.GetKey(KeyCode.A)) { movement += Vector2.left; }
        if (Input.GetKey(KeyCode.D)) { movement += Vector2.right; }

        Vector2 normalizedMovement = movement.normalized;

        Vector2 targetVelocity = normalizedMovement * moveSpeed;

        if (normalizedMovement != Vector2.zero)
        {
            rb.velocity = Vector2.MoveTowards(
                rb.velocity,
                targetVelocity,
                acceleration * Time.fixedDeltaTime
            );
        }
        else
        {
            rb.velocity = Vector2.MoveTowards(
                rb.velocity,
                Vector2.zero,
                deceleration * Time.fixedDeltaTime
            );
        }

        if (normalizedMovement != Vector2.zero)
        {
            float angle = Mathf.Atan2(normalizedMovement.y, normalizedMovement.x) * Mathf.Rad2Deg;

            if (spriteTransform != null)
            {
                spriteTransform.rotation = Quaternion.Euler(0, 0, angle - 90);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("water"))
        {
            moveSpeed = waterMoveSpeed;
        }

        if (other.gameObject.CompareTag("mountain"))
        {
            moveSpeed = mountainMoveSpeed;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("water") || other.gameObject.CompareTag("mountain"))
        {
            moveSpeed = originalMoveSpeed;
        }
    }
}