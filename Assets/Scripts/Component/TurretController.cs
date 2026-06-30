using UnityEngine;
using System.Collections;

public class TurretController : MonoBehaviour, ILvUpable, IPausable
{
    public GameObject objectToSpawn; // 生成するオブジェクトのプレハブ
    public float shootingSpeed = 5f; // 発射する速度
    public float shootingRate = 1f; // 発射レート（秒間の発射数）
    public string targetTag = "Target"; // ターゲットのタグ名

    public float shootingRange = 10f; // 射程距離

    private float shootingTimer = 0f; // 発射タイマー

    public float shootingSpreadAngle = 0f;

    [Header("Damage Settings")]
    public float minBulletDamage = 8f;
    public float maxBulletDamage = 12f;

    Bullet bullet;
    Missile missile;

    private bool isPaused = false;

    public GameObject gunTurretObj;
    public float offset; // 回転させる砲塔オブジェクト

    [Header("待機時の回転設定")]
    public float idleRotateSpeed = 90f;
    public float idleAngleChangeInterval = 2f;

    private float idleWaitTimer = 0f;
    private float idleTargetAngle = 0f;
    private bool isIdleRotating = false;

    // ===== 効果音設定 =====
    [Header("Sound Settings")]
    public AudioClip shootSound; // 発射効果音
    public float soundVolume;

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
            case 0:
                shootingSpeed *= percent;
                break;

            case 1:
                shootingRate *= percent;
                break;

            case 2:
                shootingRange *= percent;
                break;

            case 3:
                shootingSpreadAngle *= (percent - 1);
                break;

            case 4:
                minBulletDamage *= percent;
                maxBulletDamage *= percent;
                break;

            default:
                break;
        }
    }

    private void Update()
    {
        if (isPaused) return;

        // 砲塔をターゲットに向ける
        AimTurret();

        // 発射タイマーを更新
        shootingTimer += Time.deltaTime;

        // 発射レートに基づいて発射
        if (shootingTimer >= 1f / shootingRate)
        {
            ShootObject();
            shootingTimer = 0f; // タイマーをリセット
        }
    }

    // 砲塔を敵の予測位置に向ける処理
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

                Vector2 predictedPosition;
                if (targetRigidbody != null)
                {
                    predictedPosition = LinePrediction(
                        gunTurretObj.transform.position,
                        nearestTarget.transform.position,
                        targetRigidbody.velocity,
                        shootingSpeed
                    );
                }
                else
                {
                    predictedPosition = nearestTarget.transform.position;
                }

                Vector2 direction = (predictedPosition - (Vector2)gunTurretObj.transform.position).normalized;
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

    private void ShootObject()
    {
        // 一番近いターゲットを探す
        GameObject nearestTarget = FindNearestTarget();

        // ターゲットが見つかった場合のみ処理を続ける
        if (nearestTarget != null)
        {
            // ターゲットとの距離を計算
            float distance = Vector2.Distance(transform.position, nearestTarget.transform.position);

            // 距離が射程範囲内の場合のみ発射する
            if (distance <= shootingRange)
            {
                // オブジェクトを生成
                GameObject spawnedObject = Instantiate(objectToSpawn, transform.position, Quaternion.identity);

                // ===== 効果音を再生 =====
                SoundManager.Instance?.PlaySound(shootSound, transform.position, soundVolume);

                bullet = spawnedObject.GetComponent<Bullet>();
                missile = spawnedObject.GetComponent<Missile>();

                // 指定範囲内でランダムダメージを決める
                float randomDamage = Random.Range(minBulletDamage, maxBulletDamage);

                if (bullet != null)
                {
                    bullet.damage = randomDamage;
                }

                if (missile != null)
                {
                    missile.damage = randomDamage;
                }

                Debug.Log($"弾を生成しました。Damage: {randomDamage}");

                // ターゲットの Rigidbody2D を取得
                Rigidbody2D targetRigidbody = nearestTarget.GetComponent<Rigidbody2D>();

                Vector2 predictedPosition;
                if (targetRigidbody != null)
                {
                    predictedPosition = LinePrediction(
                        transform.position,
                        nearestTarget.transform.position,
                        targetRigidbody.velocity,
                        shootingSpeed
                    );
                }
                else
                {
                    predictedPosition = nearestTarget.transform.position;
                }

                // 予測位置の方向を計算
                Vector2 direction = (predictedPosition - (Vector2)transform.position).normalized;

                // 元の角度を計算
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                // ばらつきを計算
                float randomSpread = Random.Range(-shootingSpreadAngle, shootingSpreadAngle);

                // 最終的な角度を計算
                float finalAngle = angle + randomSpread;

                // オブジェクトの向きをZ軸周りに回転させる
                spawnedObject.transform.rotation = Quaternion.Euler(0, 0, finalAngle + 90);

                // 最終的な角度から発射方向を再計算
                float finalAngleRad = finalAngle * Mathf.Deg2Rad;
                Vector2 finalDirection = new Vector2(
                    Mathf.Cos(finalAngleRad),
                    Mathf.Sin(finalAngleRad)
                );

                // 生成したオブジェクトにRigidbody2Dを取得
                Rigidbody2D rb = spawnedObject.GetComponent<Rigidbody2D>();

                // 速度を設定
                if (rb != null)
                {
                    rb.velocity = finalDirection * shootingSpeed;
                }
            }
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
            if (B == 0)
            {
                return targetPosition;
            }
            else
            {
                return targetPosition + v2_Mv * (-C / B);
            }
        }

        float flame1;
        float flame2;
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
}