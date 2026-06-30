using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// 死亡時にアイテムをドロップする機能だけを持つスクリプト
public class ItemDropper : MonoBehaviour
{
    public GameObject[] itemPrefab;

    public int[] coinPercentWeight;

    [Header("ドロップ数")]
    public int dropItemCount = 1; // ドロップするアイテムの数

    [Header("ドロップ時の初速度設定")]
    public float dropForce = 1f;           // 横方向に飛び出す力
    public bool randomDirection = true;    // ランダムな方向に飛ばすか

    // このメソッドをHPManagerのイベントから呼び出してもらう
    public void DropItem()
    {
        if (itemPrefab == null || itemPrefab.Length == 0) return;
        if (coinPercentWeight == null || coinPercentWeight.Length == 0) return;

        for (int i = 0; i < dropItemCount; i++)
        {
            int index = DetermineWeightedIndex(coinPercentWeight);

            if (index < 0 || index >= itemPrefab.Length) return;

            GameObject item = Instantiate(
                itemPrefab[index],
                transform.position,
                Quaternion.identity
            );

            Rigidbody2D rb = item.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                float xDirection = randomDirection ? Random.Range(-1f, 1f) : 0f;
                float yDirection = randomDirection ? Random.Range(-1f, 1f) : 0f;

                Vector2 velocity = new Vector2(
                    xDirection * dropForce,
                    yDirection * dropForce
                );

                rb.velocity = velocity;
            }
        }
    }

    int DetermineWeightedIndex(int[] PercentArray)
    {
        int totalWeight = PercentArray.Sum();

        if (totalWeight <= 0)
        {
            return 0;
        }

        int randomInt = UnityEngine.Random.Range(0, totalWeight);
        int currentWeight = 0;

        for (int i = 0; i < PercentArray.Length; i++)
        {
            currentWeight += PercentArray[i];

            if (randomInt < currentWeight)
            {
                return i;
            }
        }

        return PercentArray.Length - 1;
    }
}