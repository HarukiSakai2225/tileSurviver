using UnityEngine;
using System.Collections;

public class MissileLauncher : MonoBehaviour, ILvUpable, IPausable
{
    [Header("発射設定")]
    public GameObject missilePrefab;
    public float launchSpeed = 3f;
    public float shootingRate = 0.5f;
    public string targetTag = "Enemy";
    public float detectionRadius = 10f;
    public Transform directionReference;

    [Header("ミサイル設定")]
    public float missileDamage = 10f;
    public float explosionRadius = 2f;
    public float missileSpeed = 5f;
    public float missileRotationSpeed = 200f;
    public float missileLifeTime = 5f;
    public float trackingDelay = 0.5f;

    [Header("Sound Settings")]
    public AudioClip shootSound;
    public float soundVolume;

    [Header("待機時の回転設定")]
    public float idleRotateSpeed = 90f;
    public float idleAngleChangeInterval = 2f;

    private float shootingTimer = 0f;
    private bool isPaused = false;

    public float offset;
    public GameObject gunTurretObj;

    private float idleWaitTimer = 0f;
    private float idleTargetAngle = 0f;
    private bool isIdleRotating = false;

    private void Awake()
    {
        if (directionReference == null && gunTurretObj != null)
        {
            directionReference = gunTurretObj.transform;
        }

        if (gunTurretObj != null)
        {
            idleTargetAngle = gunTurretObj.transform.eulerAngles.z - offset;
        }

        isIdleRotating = false;
        idleWaitTimer = 0f;
    }

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

    public void SetDirectionReference(Transform reference)
    {
        directionReference = reference;
    }

    public void LvUp(int num, float percent)
    {
        switch (num)
        {
            case 0: shootingRate *= percent; break;
            case 1: missileDamage *= percent; break;
            case 2: explosionRadius *= percent; break;
            case 3: missileSpeed *= percent; break;
            case 4: missileRotationSpeed *= percent; break;
            case 5: detectionRadius *= percent; break;
        }
    }

    private void Update()
    {
        if (isPaused) return;

        AimTurret();

        shootingTimer += Time.deltaTime;

        if (shootingTimer >= 1f / shootingRate)
        {
            if (IsTargetInRange())
            {
                LaunchMissile();
            }
            shootingTimer = 0f;
        }
    }

    private void AimTurret()
    {
        if (gunTurretObj == null) return;

        GameObject nearestTarget = FindNearestTarget();
        if (nearestTarget != null)
        {
            float distance = Vector2.Distance(transform.position, nearestTarget.transform.position);
            if (distance <= detectionRadius)
            {
                Vector2 direction = ((Vector2)nearestTarget.transform.position - (Vector2)gunTurretObj.transform.position).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                gunTurretObj.transform.rotation = Quaternion.Euler(0, 0, angle + offset);

                isIdleRotating = false;
                idleWaitTimer = 0f;
                return;
            }
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

    private bool IsTargetInRange()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);

        foreach (GameObject target in targets)
        {
            float distance = Vector2.Distance(transform.position, target.transform.position);
            if (distance <= detectionRadius)
            {
                return true;
            }
        }

        return false;
    }

    private void LaunchMissile()
    {
        Transform referenceTransform =
            directionReference != null ? directionReference :
            gunTurretObj != null ? gunTurretObj.transform :
            transform;

        Vector2 launchDirection = -referenceTransform.up;
        float angle = Mathf.Atan2(launchDirection.y, launchDirection.x) * Mathf.Rad2Deg - 90f;

        GameObject spawnedMissile = Instantiate(
            missilePrefab,
            transform.position,
            Quaternion.Euler(0, 0, angle)
        );

        SoundManager.Instance?.PlaySound(shootSound, transform.position, soundVolume);

        Missile missile = spawnedMissile.GetComponent<Missile>();
        if (missile != null)
        {
            missile.damage = missileDamage;
            missile.SetParameters(explosionRadius, missileSpeed, missileRotationSpeed, missileLifeTime, trackingDelay);
        }

        Rigidbody2D rb = spawnedMissile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = launchDirection * launchSpeed;
        }
    }

    private GameObject FindNearestTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        GameObject nearestTarget = null;
        float nearestDistance = Mathf.Infinity;

        foreach (GameObject target in targets)
        {
            float distance = Vector2.Distance(transform.position, target.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestTarget = target;
            }
        }

        return nearestTarget;
    }

    public Vector2 LinePrediction(Vector2 shotPosition, Vector2 targetPosition, Vector2 targetVelocity, float bulletSpeed)
    {
        Vector2 v2_Mv = targetVelocity;
        Vector2 v2_Pos = targetPosition - shotPosition;
        float A = (v2_Mv.x * v2_Mv.x + v2_Mv.y * v2_Mv.y) - bulletSpeed * bulletSpeed;
        float B = 2 * (v2_Pos.x * v2_Mv.x + v2_Pos.y * v2_Mv.y);
        float C = (v2_Pos.x * v2_Pos.x + v2_Pos.y * v2_Pos.y);

        if (A == 0)
        {
            if (B == 0) return targetPosition;
            return targetPosition + v2_Mv * (-C / B);
        }

        float flame1, flame2;
        float D = B * B - 4 * A * C;

        if (D > 0)
        {
            float E = Mathf.Sqrt(D);
            flame1 = (-B - E) / (2 * A);
            flame2 = (-B + E) / (2 * A);
            flame1 = PlusMin(flame1, flame2);
        }
        else
        {
            flame1 = 0;
        }

        return targetPosition + v2_Mv * flame1;
    }

    public float PlusMin(float a, float b)
    {
        if (a < 0 && b < 0) return 0;
        if (a < 0) return b;
        if (b < 0) return a;
        return a < b ? a : b;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        GameObject nearestTarget = FindNearestTarget();
        if (nearestTarget != null)
        {
            float distance = Vector2.Distance(transform.position, nearestTarget.transform.position);
            Gizmos.color = distance <= detectionRadius ? Color.red : Color.gray;

            Gizmos.DrawLine(transform.position, nearestTarget.transform.position);
            Gizmos.DrawSphere(nearestTarget.transform.position, 0.15f);
        }
    }
}