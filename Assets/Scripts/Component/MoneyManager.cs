using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; 

public class MoneyManager : MonoBehaviour, ILvUpable
{
    public GameObject target;

    [SerializeField] private float detectionRadius = 5f; // コインを引き寄せる半径
    [SerializeField] private float attractSpeed = 10f;
    [SerializeField] private string coinLayerMaskstring;
    int coinLayerMask;
    [SerializeField] float coinMRamdomLimit;
    // Start is called before the first frame update
    private GameManager gameManager;

    public AudioClip coinSound;
    public float coinSoundVolume;

    public AudioClip luckyCoinSound;
    [SerializeField] AudioSource audioSource;

    bool allCoinsAttracting = false;

    void Start()
    {
        if (gameManager == null)
        {
            // シングルトンから自動で取得する
            gameManager = GameManager.Instance;
        }
        coinLayerMask = LayerMask.GetMask(coinLayerMaskstring);
    }

    // Update is called once per frame
    void Update()
    {
        AttractNearbyCoins();
    }
    public void GetAllCoins()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 200f, coinLayerMask);
        allCoinsAttracting = true;
        StartCoroutine(AttractAllCoins(hitColliders));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 衝突した相手が "coin" レイヤーかどうかチェック
        if (((1 << other.gameObject.layer) & coinLayerMask) != 0)
        {
            // 相手が自分自身ではないことを確認
            if (other.gameObject == this.gameObject)
            {
                return;
            }

            // 1. 相手からCoinコンポーネントを取得
            Coin coin = other.gameObject.GetComponent<Coin>();


            // 2. Nullチェック
            if (coin.coinType == 0)
            {
                SoundManager.Instance?.PlaySound(coinSound, transform.position, coinSoundVolume);
                // ▼▼▼ ご依頼の修正箇所 ▼▼▼

                // 3. ランダムな倍率を計算 (0.0f ～ coinM の間)
                float randomMultiplier = Random.Range(0f, coinMRamdomLimit);

                // 4. 最終的なコインの価値を計算 (float)
                // (coin.value が int の場合を想定し、floatにキャストして計算)
                float finalValueFloat = (float)coin.value * randomMultiplier;

                // 5. OnGetCoin<int> に渡すため、float を int に変換 (四捨五入)
                int finalValueInt = Mathf.RoundToInt(finalValueFloat);

                // 6. 計算した整数値をイベントで通知
                gameManager.OnCoinCollected.Invoke(finalValueInt);

                // 7. コインのゲームオブジェクトを破棄
                Destroy(other.gameObject);
            }
            else if(coin.coinType == 1)
            {
                GetAllCoins();
                audioSource.PlayOneShot(luckyCoinSound);
                Destroy(other.gameObject);
            }
        }
    }

    private void AttractNearbyCoins()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, coinLayerMask);
        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.gameObject == this.gameObject)
            {
                continue;
            }

            Vector2 direction = (transform.position - hitCollider.transform.position).normalized;
            Rigidbody2D rb = hitCollider.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.velocity = direction * attractSpeed;
            }
        }
    }

    private IEnumerator AttractAllCoins(Collider2D[] hitColliders)
    {
        int activeCount = 0;
        while (allCoinsAttracting)
        {
            activeCount = hitColliders.Length;
            foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider == null)
            {
                activeCount--;
                continue;
            }

            Vector2 direction = (transform.position - hitCollider.transform.position).normalized;
            Rigidbody2D rb = hitCollider.GetComponent<Rigidbody2D>();
            rb.velocity = direction * attractSpeed;

        }

        if(activeCount == 0)
        allCoinsAttracting = false;
        
        yield return null;
        }
    }

    public void LvUp(int num, float percent)
    {
        switch (num)
        {
            case 0:
                detectionRadius *= percent;
                break;
            case 1:
                attractSpeed *= percent;
                break;
            default:
                break;
        }
    }
}
