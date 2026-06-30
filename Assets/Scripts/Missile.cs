using UnityEngine;
using System.Collections.Generic;

public class Missile : MonoBehaviour, IPausable
{
    [Header("ダメージ設定")]
    public float damage;
    [SerializeField] List<string> damageTags = new List<string>();
    
    [Header("爆発設定")]
    [SerializeField] float explosionRadius = 2f;
    
    [Header("追尾設定")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 200f;
    [SerializeField] float trackingDelay = 0.5f;
    
    [Header("その他")]
    [SerializeField] float lifeTime = 5.0f;

    [Header("Sound Settings")]
    [SerializeField] AudioClip explosionSound;
    public float soundVolume;

    private Transform target;
    private Rigidbody2D rb;
    private float elapsedTime = 0f;
    private bool isTracking = false;

    // ===== 一時停止機能 =====
    private bool isPaused = false;
    private Vector2 savedVelocity;
    private float savedAngularVelocity;

    // ===== ライフタイム管理 =====
    private float lifeTimeCounter = 0f;

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

    public void SetParameters(float explosionRadius, float moveSpeed, float rotationSpeed, float lifeTime, float trackingDelay)
    {
        this.explosionRadius = explosionRadius;
        this.moveSpeed = moveSpeed;
        this.rotationSpeed = rotationSpeed;
        this.lifeTime = lifeTime;
        this.trackingDelay = trackingDelay;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Destroy(gameObject, lifeTime); ← 削除
    }

    void Update()
    {
        if (isPaused) return;

        // ===== ライフタイムをカウント =====
        lifeTimeCounter += Time.deltaTime;
        if (lifeTimeCounter >= lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        elapsedTime += Time.deltaTime;

        if (!isTracking && elapsedTime >= trackingDelay)
        {
            isTracking = true;
        }

        if (isTracking && target == null)
        {
            FindNearestTarget();
        }
    }

    void FixedUpdate()
    {
        if (isPaused) return;

        if (!isTracking)
        {
            return;
        }

        if (target != null)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            float currentAngle = rb.rotation;
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.fixedDeltaTime);
            rb.rotation = newAngle;
            
            Vector2 moveDirection = transform.up;
            rb.velocity = moveDirection * moveSpeed;
        }
        else
        {
            rb.velocity = transform.up * moveSpeed;
        }
    }

    private void FindNearestTarget()
    {
        float nearestDistance = Mathf.Infinity;
        Transform nearestTarget = null;

        foreach (string tag in damageTags)
        {
            GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
            
            foreach (GameObject obj in targets)
            {
                float distance = Vector2.Distance(transform.position, obj.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestTarget = obj.transform;
                }
            }
        }

        target = nearestTarget;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (damageTags.Contains(other.tag))
        {
            Explode();
        }
    }

    private void Explode()
    {
        SoundManager.Instance?.PlaySound(explosionSound, transform.position, soundVolume);

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hit in hitColliders)
        {
            if (damageTags.Contains(hit.tag))
            {
                HPManager hpManager = hit.GetComponent<HPManager>();
                if (hpManager != null)
                {
                    hpManager.TakeDamage(damage);
                }
            }
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}