using System.Collections;
using UnityEngine;

public class MouseCursorTurretController : MonoBehaviour
{
    public GameObject objectToSpawn;
    public float shootingSpeed = 5f;
    public float shootingRate = 1f;
    public float shootingRange = 10f;
    public float shootingSpreadAngle = 0f;
    public float bulletDamage;

    [Header("バースト設定")]
    public int burstCount = 3;              // 1回のバーストで撃つ弾数
    public float burstInterval = 0.1f;      // バースト内の発射間隔（秒）

    private float shootingTimer = 0f;
    private Camera mainCamera;
    private bool isBursting = false;        // バースト中かどうか

    Bullet bullet;

    void Start()
    {
        mainCamera = Camera.main;
    }

    public void LvUp(int num, float percent)
    {
        switch (num)
        {
            case 0: shootingSpeed *= percent; break;
            case 1: shootingRate *= percent; break;
            case 2: shootingRange *= percent; break;
            case 3: shootingSpreadAngle *= (percent - 1); break;
            case 4: bulletDamage *= percent; break;
            case 5: burstCount = Mathf.RoundToInt(burstCount * percent); break;      // バースト数
            case 6: burstInterval /= percent; break;                                  // バースト間隔短縮
            default: break;
        }
    }

    private void Update()
    {
        shootingTimer += Time.deltaTime;

        if (shootingTimer >= 1f / shootingRate && !isBursting)
        {
            StartCoroutine(BurstShoot());
            shootingTimer = 0f;
        }
    }

    private IEnumerator BurstShoot()
    {
        isBursting = true;

        for (int i = 0; i < burstCount; i++)
        {
            ShootObject();

            // 最後の弾以外は間隔を空ける
            if (i < burstCount - 1)
            {
                yield return new WaitForSeconds(burstInterval);
            }
        }

        isBursting = false;
    }

    private void ShootObject()
    {
        // マウスのワールド座標を取得
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // マウス方向を計算
        Vector2 direction = (mousePosition - (Vector2)transform.position).normalized;

        // 角度を計算
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // ばらつきを適用
        float randomSpread = Random.Range(-shootingSpreadAngle, shootingSpreadAngle);
        float finalAngle = angle + randomSpread;

        // 最終的な方向ベクトルを計算
        float finalAngleRad = finalAngle * Mathf.Deg2Rad;
        Vector2 finalDirection = new Vector2(Mathf.Cos(finalAngleRad), Mathf.Sin(finalAngleRad));

        // 弾を生成
        GameObject spawnedObject = Instantiate(objectToSpawn, transform.position, Quaternion.identity);
        spawnedObject.transform.rotation = Quaternion.Euler(0, 0, finalAngle + 90);

        bullet = spawnedObject.GetComponent<Bullet>();
        bullet.damage = bulletDamage;

        Rigidbody2D rb = spawnedObject.GetComponent<Rigidbody2D>();
        rb.velocity = finalDirection * shootingSpeed;
    }
}