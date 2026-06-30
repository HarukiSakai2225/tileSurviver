using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HPManager : MonoBehaviour, ILvUpable
{
    [Header("HP Stats")]
    public float maxHp = 100;
    public float currentHp;
    public float healRate = 1; // 1秒間に回復する回数
    public float healAmount = 1; // 1回あたりの回復量

    [Header("イベント")]
    public UnityEvent<float, float> OnHealthChanged = new UnityEvent<float, float>();
    public UnityEvent<float> OnTakeDamage = new UnityEvent<float>();
    public UnityEvent OnDeath = new UnityEvent();
    public UnityEvent<float, float> OnEMPed = new UnityEvent<float, float>();

    private bool isDead = false;

    [SerializeField] bool isPlayer = false;

    private GameManager gameManager;

    int originalPCost;
    Vector2Int originalPosition;
    bool isPlayerTile = false;

    // ▼▼▼ 追加：タイマー管理用変数 ▼▼▼
    private float healTimer = 0f;
    // ▲▲▲ 追加 ▲▲▲

    void Start()
    {
        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
        }

        if (gameManager == null)
        {
            Debug.LogError("GameManager がシーン内に見つかりません！");
        }

        currentHp = maxHp;
        OnHealthChanged.Invoke(currentHp, maxHp);

        if(isPlayer)
            gameManager.OnPlayerSpawn.Invoke(transform.gameObject);

        // ★コルーチンの開始（StartCoroutine）は削除しました
    }

    // ▼▼▼ 追加：Updateで時間を計測して回復する ▼▼▼
    void Update()
    {
        // 死亡していたり、回復速度が0以下の場合は何もしない
        if (isDead || healRate <= 0) return;

        // タイマーを進める
        healTimer += Time.deltaTime;

        // 「1秒 / 回数」で、回復に必要な間隔(秒)を計算
        // 例: healRate=2 なら 0.5秒ごとに回復
        float interval = 1.0f / healRate;

        // タイマーが間隔を超えたら回復実行
        if (healTimer >= interval)
        {
            Heal(healAmount);
            
            // タイマーをリセット
            // (単に0にするより、intervalを引くほうが時間のズレが少なくなりますが、
            //  簡易的に 0 にリセットでもゲーム的には問題ありません)
            healTimer = 0f; 
        }
    }
    // ▲▲▲ 追加 ▲▲▲

    public void EMPed(float percent, float duration)
    {
        OnEMPed.Invoke(percent, duration);
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHp -= damageAmount;
        if (currentHp < 0) currentHp = 0;

        OnHealthChanged.Invoke(currentHp, maxHp);
        OnTakeDamage.Invoke(damageAmount);

        if (currentHp <= 0)
        {
            isDead = true;
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        if (isDead) return;

        currentHp += healAmount;
        if (currentHp > maxHp) currentHp = maxHp;

        OnHealthChanged.Invoke(currentHp, maxHp);
    }

    // ★ IEnumerator HealRoutine() は削除しました

    private void Die()
    {
        OnDeath.Invoke();

        if(isPlayer)
        {
            gameManager.OnPlayerDied.Invoke();
        }
        else if(!isPlayerTile)
        {
            enemySpawnManager.Instance.decreaseSpawnedCount();
        }
        
        if(isPlayerTile)
            SetOriginalCost();

        Destroy(gameObject);
    }

    void SetOriginalCost()
    {
        // Nullチェックを追加しておくと安全です
        if (MapGenerator.Instance != null && MapGenerator.Instance.grid != null)
        {
             MapGenerator.Instance.grid[originalPosition.x, originalPosition.y].PCost = originalPCost;
        }
    }

    public void SetTile(Vector2Int pos, int cost)
    {
        originalPosition = pos;
        originalPCost = cost;
        isPlayerTile = true;
    }


    public void LvUp(int num, float percent)
    {
        switch (num)
        {
            case 0:
                maxHp *= percent;
                // HP最大値が増えたとき、現在のHPも割合で増やすか、
                // そのままにするかは仕様次第ですが、現状はそのままでOKです。
                // UI更新を入れたい場合はここで OnHealthChanged.Invoke を呼ぶのもアリです。
                OnHealthChanged.Invoke(currentHp, maxHp); 
                break;
            case 1:
                healRate *= percent;
                break;
            case 2:
                healAmount *= percent;
                break;
            default:
                break;
        }
    }
}