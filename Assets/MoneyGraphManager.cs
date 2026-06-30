using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoneyGraphManager : MonoBehaviour
{
    [SerializeField]
    private LvUpManager lvUpManager;

    [SerializeField] GameObject Graph;

    RectTransform rectTransform;

    [SerializeField] TextMeshProUGUI levelUpCounterText;
    [SerializeField] TextMeshProUGUI totalMoneyText;
    [SerializeField] TextMeshProUGUI coinChangeText; // 変化量用のテキスト（別途追加）
    int currentCoinAmount = 0;

    // 色の設定
    [SerializeField] Color plusColor = Color.green;
    [SerializeField] Color minusColor = Color.red;
    
    // フェード用
    Coroutine fadeCoroutine;

    void Awake()
    {
        rectTransform = Graph.GetComponent<RectTransform>();
    }

    public void UpdateGraph(LvUpManager.CoinData data)
    {
        int CoinChange = data.totalCoinAmount - currentCoinAmount;
        currentCoinAmount = data.totalCoinAmount;
        float radio = 760f / data.coinAmountforLvUp;
        float length = 780f - radio * data.currentCoinAmount;
        rectTransform.offsetMax = new Vector2(length * -1, rectTransform.offsetMax.y);
        levelUpCounterText.text = $"{data.currentCoinAmount}/{data.coinAmountforLvUp} Lv{data.level}";
        
        // 合計金額（白で固定）
        totalMoneyText.color = Color.white;
        totalMoneyText.text = $"{data.totalCoinAmount}";
        
        // 変化量の表示とフェード
        if (CoinChange != 0)
        {
            // 前回のフェードを停止
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            
            // 色とテキストを設定
            if (CoinChange > 0)
            {
                coinChangeText.color = plusColor;
                coinChangeText.text = $"+{CoinChange}";
            }
            else
            {
                coinChangeText.color = minusColor;
                coinChangeText.text = $"{CoinChange}";
            }
            
            // フェードアウト開始
            fadeCoroutine = StartCoroutine(FadeOutText(coinChangeText, 1f));
        }
    }

    IEnumerator FadeOutText(TextMeshProUGUI text, float duration)
    {
        Color startColor = text.color;
        startColor.a = 1f;
        text.color = startColor;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Color c = text.color;
            c.a = Mathf.Lerp(1f, 0f, elapsed / duration);
            text.color = c;
            yield return null;
        }
        
        // 完全に透明に
        Color finalColor = text.color;
        finalColor.a = 0f;
        text.color = finalColor;
    }
}