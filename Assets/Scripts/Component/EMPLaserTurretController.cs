using UnityEngine;
using System.Collections;

public class EMPLaserTurretController : MonoBehaviour, ILvUpable, IPausable
{
    [Header("発射設定")]
    public float shootingRate = 1f;
    public float shootingRange = 10f;
    public float laserWidth = 0.5f;
    public float laserDamage = 10f;
    public float slowPercent = 30f;
    public float slowDuration = 5f;

    [Header("索敵設定")]
    public string targetTag = "Enemy";
    public LayerMask detectionLayer;

    [Header("レーザー表示")]
    public LineRenderer lineRenderer;
    public float laserDuration = 0.1f;

    [Header("Sound Settings")]
    public AudioClip shootSound;
    public float soundVolume;

    [Header("砲塔設定")]
    public GameObject gunTurretObj;
    public float offset;

    [Header("待機時の回転設定")]
    public float idleRotateSpeed = 90f;
    public float idleAngleChangeInterval = 2f;

    [Header("デバッグ")]
    public bool showGizmos = true;

    private float shootingTimer = 0f;
    private bool isLaserActive = false;
    private float laserTimer = 0f;
    private Vector2 lastLaserEndPosition;

    private bool isPaused = false;

    private float idleWaitTimer = 0f;
    private float idleTargetAngle = 0f;
    private bool isIdleRotating = false;

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
    }

    public void OnResume()
    {
        isPaused = false;
    }

    void Start()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            lineRenderer.positionCount = 2;
        }

        if (gunTurretObj != null)
        {
            idleTargetAngle = gunTurretObj.transform.eulerAngles.z - offset;
        }

        isIdleRotating = false;
        idleWaitTimer = 0f;
    }

    public void LvUp(int num, float percent)
    {
        switch (num)
        {
            case 0: shootingRate *= percent; break;
            case 1: shootingRange *= percent; break;
            case 2: laserWidth *= percent; break;
            case 3: laserDamage *= percent; break;
            case 4: slowPercent *= percent; break;
            case 5: slowDuration *= percent; break;
            default: break;
        }
    }

    void Update()
    {
        if (isPaused) return;

        GameObject target = FindNearestTargetInRange();

        AimTurret(target);

        shootingTimer += Time.deltaTime;

        if (shootingTimer >= 1f / shootingRate)
        {
            if (target != null)
            {
                Vector2 direction = ((Vector2)target.transform.position - (Vector2)transform.position).normalized;
                FireLaser(direction);
            }

            shootingTimer = 0f;
        }

        if (isLaserActive)
        {
            laserTimer += Time.deltaTime;
            if (laserTimer >= laserDuration)
            {
                DisableLaser();
            }
        }
    }

    private void AimTurret(GameObject target)
    {
        if (gunTurretObj == null) return;

        if (target != null)
        {
            Vector2 direction = ((Vector2)target.transform.position - (Vector2)gunTurretObj.transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            gunTurretObj.transform.rotation = Quaternion.Euler(0, 0, angle + offset);

            isIdleRotating = false;
            idleWaitTimer = 0f;
            return;
        }

        if (!isIdleRotating)
        {
            idleWaitTimer += Time.deltaTime;

            if (idleWaitTimer >= idleAngleChangeInterval)
            {
                idleTargetAngle = Random.Range(0f, 360f);
                isIdleRotating = true;
                idleWaitTimer = 0f;
            }

            return;
        }

        Quaternion targetRotation = Quaternion.Euler(0, 0, idleTargetAngle + offset);
        gunTurretObj.transform.rotation = Quaternion.RotateTowards(
            gunTurretObj.transform.rotation,
            targetRotation,
            idleRotateSpeed * Time.deltaTime
        );

        float angleDiff = Quaternion.Angle(gunTurretObj.transform.rotation, targetRotation);
        if (angleDiff < 0.1f)
        {
            gunTurretObj.transform.rotation = targetRotation;
            isIdleRotating = false;
            idleWaitTimer = 0f;
        }
    }

    void FireLaser(Vector2 direction)
    {
        Vector2 pointA = transform.position;
        Vector2 pointB = (Vector2)transform.position + direction * shootingRange;

        lastLaserEndPosition = pointB;

        Vector2 center = (pointA + pointB) / 2f;
        float length = shootingRange;
        Vector2 size = new Vector2(length, laserWidth);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Collider2D[] colliders = Physics2D.OverlapBoxAll(center, size, angle, detectionLayer);

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag(targetTag))
            {
                HPManager hp = col.GetComponent<HPManager>();
                if (hp != null)
                {
                    hp.TakeDamage(laserDamage);
                    hp.EMPed(slowPercent, slowDuration);
                }
            }
        }

        SoundManager.Instance?.PlaySound(shootSound, transform.position, soundVolume);

        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
            lineRenderer.startWidth = laserWidth;
            lineRenderer.endWidth = laserWidth;
            lineRenderer.SetPosition(0, pointA);
            lineRenderer.SetPosition(1, pointB);
            isLaserActive = true;
            laserTimer = 0f;
        }
    }

    void DisableLaser()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
        isLaserActive = false;
    }

    GameObject FindNearestTargetInRange()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        GameObject nearestTarget = null;
        float nearestDistance = Mathf.Infinity;

        foreach (GameObject target in targets)
        {
            float distance = Vector2.Distance(transform.position, target.transform.position);
            if (distance <= shootingRange && distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestTarget = target;
            }
        }

        return nearestTarget;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, shootingRange);

        GameObject nearestTarget = FindNearestTargetInRange();
        if (nearestTarget != null)
        {
            float distance = Vector2.Distance(transform.position, nearestTarget.transform.position);
            Gizmos.color = distance <= shootingRange ? Color.red : Color.gray;
            Gizmos.DrawLine(transform.position, nearestTarget.transform.position);
            Gizmos.DrawSphere(nearestTarget.transform.position, 0.15f);
        }

        if (showGizmos && isLaserActive)
        {
            Vector2 pointA = transform.position;
            Vector2 pointB = lastLaserEndPosition;

            Vector2 center = (pointA + pointB) / 2f;
            float length = shootingRange;
            Vector2 size = new Vector2(length, laserWidth);

            Vector2 direction = pointB - pointA;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            Gizmos.color = Color.yellow;
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(center, Quaternion.Euler(0, 0, angle), Vector3.one);
            Gizmos.matrix = rotationMatrix;
            Gizmos.DrawWireCube(Vector3.zero, size);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}