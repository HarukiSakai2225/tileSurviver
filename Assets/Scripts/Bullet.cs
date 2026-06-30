using UnityEngine;
using System.Collections.Generic;

public class Bullet : MonoBehaviour
{
    public float damage;
    [SerializeField] List<string> damageTags = new List<string>();
    [SerializeField] float lifeTime = 5.0f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // Update() は不要
    // void Update()
    // {
    // }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // タグのチェック（ここは変更なし）
        if (damageTags.Contains(other.tag))
        {
            // 相手のHPManagerを取得（ここは変更なし）
            HPManager hPManager = other.GetComponent<HPManager>();

            // 相手にHPManagerがある場合
            if (hPManager != null)
            {
                // ★★★ ここが最大の変更点 ★★★
                // 悪い例：hPManager.hp -= damage; (変数を直接操作＝密結合)
                
                // 良い例：hPManager.TakeDamage(damage); (メソッドを呼び出す＝疎結合)
                // ------------------------------------
                // 「いくらダメージを与えるか」だけを伝え、
                // 実際のHP計算（防御力など）はHPManagerに任せる。
                hPManager.TakeDamage(damage);
            }

            // 自分自身（弾）を破棄する（ここは変更なし）
            Destroy(gameObject);
        }
    }
}