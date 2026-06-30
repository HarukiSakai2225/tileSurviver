using System.Collections;
using UnityEngine;

public class BurstTurretController : MonoBehaviour, ILvUpable, IPausable
{
    [Header("発射設定")]
    public GameObject objectToSpawn;
    public float shootingSpeed = 5f;
    public float shootingRate = 1f;
    public float shootingRange = 10f;
    public float shootingSpreadAngle = 0f;
    public float bulletDamage;

    [Header("索敵設定")]
    public string targetTag = "Enemy";

    [Header("バースト設定")]
    public float burstCount = 3;
    public float burstInterval = 0.033f;

    [Header("Sound Settings")]
    public AudioClip shootSound;
    public float soundVolume;

    private float shootingTimer = 0f;
    private bool isBursting = false;
    private Vector2 burstTargetDirection;

    Bullet bullet;
    Missile missile;

    public float offset; 
    public GameObject gunTurretObj;

    // ===== 一時停止機能 =====
    private bool isPaused = false;

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

    public void LvUp(int num, float percent)
    {
        switch (num)
        {
            case 0: shootingSpeed *= percent; break;
            case 1: shootingRate *= percent; break;
            case 2: shootingRange *= percent; break;
            case 3: bulletDamage *= percent; break;
            case 4:
                burstCount *= percent;
                burstInterval = 0.06f / burstCount;
                break;
            default: break;
        }
    }

    private void Update()
    {
        if (isPaused) return;

        AimTurret();

        shootingTimer += Time.deltaTime;

        if (shootingTimer >= 1f / shootingRate && !isBursting)
        {
            GameObject target = FindNearestTarget();
            if (target != null)
            {
                float distance = Vector2.Distance(transform.position, target.transform.position);
                if (distance <= shootingRange)
                {
                    StartCoroutine(BurstShoot(target));
                }
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

            if (distance <= shootingRange)
            {
                Rigidbody2D targetRigidbody = nearestTarget.GetComponent<Rigidbody2D>();

                // 偏差射撃の予測位置を計算
                Vector2 predictedPosition = LinePrediction(gunTurretObj.transform.position, nearestTarget.transform.position, targetRigidbody.velocity, shootingSpeed);

                // 予測位置への方向を計算
                Vector2 direction = (predictedPosition - (Vector2)gunTurretObj.transform.position).normalized;

                // 角度を計算
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                // 砲塔を回転させる (弾の生成時と同じく +90 のオフセットを適用)
                gunTurretObj.transform.rotation = Quaternion.Euler(0, 0, angle + offset);
            }
        }
    }

    private IEnumerator BurstShoot(GameObject target)
    {
        isBursting = true;

        SoundManager.Instance?.PlaySound(shootSound, transform.position, soundVolume);
        CalculateBurstTargetDirection(target);

        for (int i = 0; i < burstCount; i++)
        {
            // ===== 一時停止中は待機 =====
            while (isPaused)
            {
                yield return null;
            }

            ShootObject();

            if (i < burstCount - 1)
            {
                yield return new WaitForSeconds(burstInterval);
            }
        }

        isBursting = false;
    }

    private void CalculateBurstTargetDirection(GameObject target)
    {
        if (target == null)
        {
            burstTargetDirection = Vector2.right;
            return;
        }

        Rigidbody2D targetRigidbody = target.GetComponent<Rigidbody2D>();

        if (targetRigidbody != null)
        {
            Vector2 predictedPosition = LinePrediction(
                transform.position,
                target.transform.position,
                targetRigidbody.velocity,
                shootingSpeed
            );
            burstTargetDirection = (predictedPosition - (Vector2)transform.position).normalized;
        }
        else
        {
            burstTargetDirection = ((Vector2)target.transform.position - (Vector2)transform.position).normalized;
        }
    }

    private void ShootObject()
    {
        Vector2 direction = burstTargetDirection;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float randomSpread = Random.Range(-shootingSpreadAngle, shootingSpreadAngle);
        float finalAngle = angle + randomSpread;

        float finalAngleRad = finalAngle * Mathf.Deg2Rad;
        Vector2 finalDirection = new Vector2(Mathf.Cos(finalAngleRad), Mathf.Sin(finalAngleRad));

        GameObject spawnedObject = Instantiate(objectToSpawn, transform.position, Quaternion.identity);
        spawnedObject.transform.rotation = Quaternion.Euler(0, 0, finalAngle + 90);

        bullet = spawnedObject.GetComponent<Bullet>();
        missile = spawnedObject.GetComponent<Missile>();

        if (bullet != null)
        {
            bullet.damage = bulletDamage;
        }

        if (missile != null)
        {
            missile.damage = bulletDamage;
        }

        Rigidbody2D rb = spawnedObject.GetComponent<Rigidbody2D>();
        rb.velocity = finalDirection * shootingSpeed;
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
            if (B == 0)
            {
                return targetPosition;
            }
            else
            {
                return targetPosition + v2_Mv * (-C / B);
            }
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
        Gizmos.DrawWireSphere(transform.position, shootingRange);
    }
}