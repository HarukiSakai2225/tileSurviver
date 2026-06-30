using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; 
using System.Linq;
using System;

[System.Serializable]
public class SelectedLvUpData
{
    public int TargetIndexNum = -1;
    public int AssetIndexNum = -1;
    public int StatusIndexNum = -1;
    public float StatusPercent = 0f;
    public int FunctionIndexNum = -1;
    public int ComponentIndexNum = -1;
}

public struct ButtonData
{
    public string buttonstr;
    public Sprite buttonSprite;
    public Sprite targetButtonSprite;
}

[System.Serializable]
public class GameFunctionData
{
    public string targetName; 
    public Sprite targetImage;
    public bool IsUnlocked;
    public int UnlockingPercentWeight;
    public int UnlockingComponentPercentWeight;
    public int LvUpPercentWeight;

    public List<GameObject> targetObjList = new List<GameObject>();

    [System.Serializable]
    public class GeneralLvUpDataAssetData
    {
        public List<GeneralLvUpDataAsset> AssetArray;
        public List<int> AssetPercentWeight;
    }
    public GeneralLvUpDataAssetData generalLvUpDataAssetData;

    [System.Serializable]
    public class GeneralLockedLvUpDataAssetData
    {
        public GeneralLvUpDataAsset[] AssetArray;
        public int[] AssetPercentWeight;

        public UnlockComponentAsset[] ComponentAssetArray;
        public int[] UnlockingComponentPercentWeight;
    }
    public GeneralLockedLvUpDataAssetData generalLockedLvUpDataAssetData;

    public UnlockFunctionAsset UFAsset;
}

public class LvUpManager : MonoBehaviour
{
    [System.Serializable]
    public struct CoinData
    {
        public int currentCoinAmount;
        public int totalCoinAmount;
        public int coinAmountforLvUp;
        public int level;
    }

    public static LvUpManager Instance { get; private set; }

    public GameFunctionData[] GameFunctionDataArray; 
    public List<GameFunctionData> UnlockedGameFunctionDataArray; 

    List<int> UFLvUpPercentWeightArray;
    List<SelectedLvUpData> SelectedLvUpDataList;

    public bool OnLvUp = false;

    [SerializeField] int currentCoinAmount = 0; 
    [SerializeField] int totalCoinAmount = 0;   
    private int level = 1;   
    public int levelUpCount;

    public GameObject playerObj;
    public GameObject core;

    public UnityEvent<CoinData> OnCoinDataUpdated = new UnityEvent<CoinData>();

    [Header("レベルアップの状態")]
    [SerializeField] private int coinAmountforLvUp = 1000;
    [SerializeField] private float costMultiplier = 1.1f;

    public int[] firstPercentWeightArray;
    List<int> UnlockingFunctionPercentWeightArray;
    List<int> UnlockingComponentPercentWeightArray;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        PublishCoinData();

        UnlockedGameFunctionDataArray = new List<GameFunctionData>();
        UFLvUpPercentWeightArray = new List<int>();
        SelectedLvUpDataList = new List<SelectedLvUpData>();
        UnlockingFunctionPercentWeightArray = new List<int>();
        UnlockingComponentPercentWeightArray = new List<int>();

        foreach (var item in GameFunctionDataArray)
        {
            if (item.IsUnlocked)
            {
                UnlockedGameFunctionDataArray.Add(item);
                UFLvUpPercentWeightArray.Add(item.LvUpPercentWeight);
                item.UnlockingPercentWeight = 0;
            }
            UnlockingFunctionPercentWeightArray.Add(item.UnlockingPercentWeight);
        }

        foreach (var item in UnlockedGameFunctionDataArray)
        {
            UnlockingComponentPercentWeightArray.Add(item.UnlockingComponentPercentWeight);
        }
    }

    public void HandleCoinCollected(int coinValue)
    {
        currentCoinAmount += coinValue;
        totalCoinAmount += coinValue;

        while (currentCoinAmount >= coinAmountforLvUp)
        {
            currentCoinAmount -= coinAmountforLvUp;
            coinAmountforLvUp = Mathf.RoundToInt(coinAmountforLvUp * costMultiplier);
            level++;
            levelUpCount++;
            TryDecideLvUp();
        }

        PublishCoinData();
    }

    public void TryDecideLvUp()
    {
        if (levelUpCount >= 1 && !OnLvUp)
        {
            DecideLvUp();
        }
    }

    void DecideLvUp()
    {
        OnLvUp = true;
        SelectedLvUpDataList.Clear();

        for (int i = 0; i < 3; i++)
        {
            // 1. まず強化タイプ（0:機能解放, 1:コンポーネント解放, 2:ステータス強化）を抽選
            int typeIndex = DetermineWeightedIndex(firstPercentWeightArray);

            // --- フォールバック判定エリア ---
            // ケース0: 機能解放が選ばれたが、解放可能な機能がない場合
            if (typeIndex == 0)
            {
                int testIndex = DetermineWeightedIndex(UnlockingFunctionPercentWeightArray);
                if (testIndex == -1)
                {
                    typeIndex = 2; // 強制的にステータス強化へ
                }
            }
            
            // ケース1: コンポーネント解放が選ばれたが、対象がない場合
            if (typeIndex == 1)
            {
                int testIndex = DetermineWeightedIndex(UnlockingComponentPercentWeightArray);
                if (testIndex == -1)
                {
                    typeIndex = 2; // 強制的にステータス強化へ
                }
            }

            // --- 確定処理エリア ---
            
            // A. 機能解放 (Unlock Function)
            if (typeIndex == 0)
            {
                int UnlockingIndex = DetermineWeightedIndex(UnlockingFunctionPercentWeightArray);
                
                if (UnlockingIndex != -1)
                {
                    SelectedLvUpData data = new SelectedLvUpData
                    {
                        FunctionIndexNum = UnlockingIndex
                    };
                    SelectedLvUpDataList.Add(data);

                    ButtonData buttonData = new ButtonData
                    {
                        buttonstr = GameFunctionDataArray[UnlockingIndex].targetName + "が使えるようになる",
                        buttonSprite = null,
                        targetButtonSprite = GameFunctionDataArray[UnlockingIndex].targetImage
                    };
                    ButtonManager.Instance.GetString(buttonData);
                }
                else
                {
                    typeIndex = 2; 
                }
            }

            // B. コンポーネント解放 (Unlock Component)
            if (typeIndex == 1) 
            {
                int UnlockingIndex = DetermineWeightedIndex(UnlockingComponentPercentWeightArray);

                if (UnlockingIndex != -1)
                {
                    var targetFunction = UnlockedGameFunctionDataArray[UnlockingIndex];
                    int UnlockingComIndex = DetermineWeightedIndex(targetFunction.generalLockedLvUpDataAssetData.UnlockingComponentPercentWeight);

                    if (UnlockingComIndex != -1)
                    {
                        SelectedLvUpData data = new SelectedLvUpData
                        {
                            TargetIndexNum = UnlockingIndex,
                            ComponentIndexNum = UnlockingComIndex
                        };
                        SelectedLvUpDataList.Add(data);

                        ButtonData buttonData = new ButtonData
                        {
                            buttonstr = targetFunction.targetName + "が" + targetFunction.generalLockedLvUpDataAssetData.ComponentAssetArray[UnlockingComIndex].componentName + "を使えるようになる",
                            buttonSprite = targetFunction.generalLockedLvUpDataAssetData.ComponentAssetArray[UnlockingComIndex].image[0],
                            targetButtonSprite = targetFunction.targetImage
                        };
                        ButtonManager.Instance.GetString(buttonData);
                    }
                    else
                    {
                        typeIndex = 2; // ステータス強化へ
                    }
                }
                else
                {
                    typeIndex = 2; // ステータス強化へ
                }
            }

            // C. ステータス強化 (Status Upgrade)
            if (typeIndex == 2)
            {
                int targetIndex = DetermineWeightedIndex(UFLvUpPercentWeightArray);
                
                if (targetIndex != -1)
                {
                    SelectIndex(targetIndex);
                }
                else
                {
                    Debug.LogWarning("強化可能な対象（UnlockedGameFunction）が存在しません。初期装備の設定を確認してください。");
                }
            }
        }
        levelUpCount--;
    }

    void SelectIndex(int targetIndex)
    {
        // 1. アセットとステータスを抽選
        int AssetIndex = DetermineWeightedIndex(UnlockedGameFunctionDataArray[targetIndex].generalLvUpDataAssetData.AssetPercentWeight);
        
        GeneralLvUpDataAsset selectedAsset = UnlockedGameFunctionDataArray[targetIndex].generalLvUpDataAssetData.AssetArray[AssetIndex];
        int StatusIndex = DetermineWeightedIndex(selectedAsset.StatusPercentWeight);

        // 2. ランダムなパーセンテージを計算
        float maxPercent = selectedAsset.maxRandomNum[StatusIndex];
        float displayRandomPercent = FindClosestElement(UnityEngine.Random.Range(1f, maxPercent), selectedAsset.LvUpPercentValue); 
        float randomPercent = displayRandomPercent / 100f + 1;
        
        // 3. 選択データをリストに追加
        SelectedLvUpData data = new SelectedLvUpData
        {
            TargetIndexNum = targetIndex,
            AssetIndexNum = AssetIndex,
            StatusIndexNum = StatusIndex,
            StatusPercent = randomPercent
        };
        SelectedLvUpDataList.Add(data);

        // 4. 文字列の構築
        string targetName = UnlockedGameFunctionDataArray[targetIndex].targetName;
        string statusName = selectedAsset.StatusName[StatusIndex];
        string percentString = displayRandomPercent.ToString("F0"); 

        List<string> valuesToInsert = new List<string>
        {
            targetName,
            statusName,
            percentString
        };

        string[] lvUpStringTemplate = (string[])selectedAsset.LvUpString.Clone(); 

        for (int i = 0; i < lvUpStringTemplate.Length; i++)
        {
            if (string.IsNullOrEmpty(lvUpStringTemplate[i]))
            {
                if (valuesToInsert.Count > 0)
                {
                    lvUpStringTemplate[i] = valuesToInsert[0];
                    valuesToInsert.RemoveAt(0);
                }
                else
                {
                    break;
                }
            }
        }

        ButtonData buttonData = new ButtonData
        {
            buttonstr = string.Concat(lvUpStringTemplate),
            buttonSprite = selectedAsset.image[StatusIndex + 1],
            targetButtonSprite = UnlockedGameFunctionDataArray[targetIndex].targetImage
        };
        ButtonManager.Instance.GetString(buttonData);
    }

    public void DoLvUp(int index)
    {
        SelectedLvUpData selectedData = SelectedLvUpDataList[index];

        // ステータス強化ではない（＝機能解放 or コンポーネント解放）
        if (selectedData.StatusIndexNum == -1)
        {
            // 新規機能（タレットなど）の解放
            if (selectedData.ComponentIndexNum == -1)
            {
                var targetFunction = GameFunctionDataArray[selectedData.FunctionIndexNum];
                
                // 抽選対象から外す（もう解放したので）
                targetFunction.UnlockingPercentWeight = 0;
                
                // 解放済みリストに追加
                UnlockedGameFunctionDataArray.Add(targetFunction);

                // 強化抽選リストにも追加
                UFLvUpPercentWeightArray.Add(targetFunction.LvUpPercentWeight);
                
                // コンポーネント解放抽選リストにも追加
                UnlockingComponentPercentWeightArray.Add(targetFunction.UnlockingComponentPercentWeight);

                // 実際の解放処理
                targetFunction.UFAsset.UnlockFunction();

                // まだロックされている機能の抽選リストを再構築
                UnlockingFunctionPercentWeightArray.Clear();
                foreach (var item in GameFunctionDataArray)
                {
                    UnlockingFunctionPercentWeightArray.Add(item.UnlockingPercentWeight);
                }
            }
            // 既存機能のコンポーネント（特殊能力など）解放
            else
            {
                var targetData = UnlockedGameFunctionDataArray[selectedData.TargetIndexNum];
                var lockedData = targetData.generalLockedLvUpDataAssetData;
                var unlockedData = targetData.generalLvUpDataAssetData;

                // コンポーネントの抽選確率を0にする（取得済み）
                lockedData.UnlockingComponentPercentWeight[selectedData.ComponentIndexNum] = 0;

                // 型が一致している前提で追加
                unlockedData.AssetArray.Add(lockedData.AssetArray[selectedData.ComponentIndexNum]);
                unlockedData.AssetPercentWeight.Add(lockedData.AssetPercentWeight[selectedData.ComponentIndexNum]);

                // コンポーネントの実処理を実行
                lockedData.ComponentAssetArray[selectedData.ComponentIndexNum].UnlockComponent(targetData.targetObjList);

                // この機能のコンポーネントがまだ残っているか確認
                int totalWeight = lockedData.UnlockingComponentPercentWeight.Sum();

                // もし残りのコンポーネントがなければ、この機能自体のコンポーネント抽選確率を0にする
                if (totalWeight == 0)
                {
                    targetData.UnlockingComponentPercentWeight = 0;
                    
                    // 全体のコンポーネント抽選リストを再構築
                    UnlockingComponentPercentWeightArray.Clear();
                    foreach (var item in UnlockedGameFunctionDataArray)
                    {
                        UnlockingComponentPercentWeightArray.Add(item.UnlockingComponentPercentWeight);
                    }
                }
            }
        }
        else
        {
            // 通常のステータス強化
            List<GameObject> targetList = UnlockedGameFunctionDataArray[selectedData.TargetIndexNum].targetObjList;
            
            GeneralLvUpDataAsset assetToUse = UnlockedGameFunctionDataArray[selectedData.TargetIndexNum]
                .generalLvUpDataAssetData.AssetArray[selectedData.AssetIndexNum];

            assetToUse.LvUp(targetList, selectedData.StatusIndexNum, selectedData.StatusPercent);
        }
        OnLvUp = false;
    }

    public void SetItemLists(
        List<GameObject> gunTurrets, 
        List<GameObject> burstTurrets,
        List<GameObject> missileTurrets,
        List<GameObject> empLaserTurrets,
        List<GameObject> barriers,
        List<GameObject> diggers)
    {
        // インデックスとリストの対応（GameFunctionDataArrayの順序に合わせて調整してください）
        // 0: プレイヤー, 1: ガンタレット, 2: バースト, 3: ミサイル, 4: EMP, 5: バリア, 6: コア, 7: ディガー
        
        if (GameFunctionDataArray.Length > 0) 
            GameFunctionDataArray[0].targetObjList = new List<GameObject> { playerObj };
        
        if (GameFunctionDataArray.Length > 1) 
            GameFunctionDataArray[1].targetObjList = gunTurrets;
        
        if (GameFunctionDataArray.Length > 2) 
            GameFunctionDataArray[2].targetObjList = burstTurrets;
        
        if (GameFunctionDataArray.Length > 3) 
            GameFunctionDataArray[3].targetObjList = missileTurrets;
        
        if (GameFunctionDataArray.Length > 4) 
            GameFunctionDataArray[4].targetObjList = empLaserTurrets;
        
        if (GameFunctionDataArray.Length > 5) 
            GameFunctionDataArray[5].targetObjList = barriers;
        
        if (GameFunctionDataArray.Length > 6) 
            GameFunctionDataArray[6].targetObjList = new List<GameObject> { core };
        
        if (GameFunctionDataArray.Length > 7) 
            GameFunctionDataArray[7].targetObjList = diggers;
    }

    private void PublishCoinData()
    {
        CoinData data = new CoinData 
        {
            currentCoinAmount = this.currentCoinAmount,
            totalCoinAmount = this.totalCoinAmount,
            coinAmountforLvUp = this.coinAmountforLvUp,
            level = this.level
        };
        OnCoinDataUpdated.Invoke(data);
    }

    // ---------------------------------------------------------
    // 修正3: バグ回避版のDetermineWeightedIndexを適用
    // ---------------------------------------------------------
    int DetermineWeightedIndex(IList<int> percentList)
    {
        if (percentList == null || percentList.Count == 0)
        {
            return -1;
        }

        int totalWeight = percentList.Sum();
        if (totalWeight <= 0)
        {
            return -1;
        }

        int randomInt = UnityEngine.Random.Range(0, totalWeight);
        int currentWeight = 0;

        for (int i = 0; i < percentList.Count; i++)
        {
            currentWeight += percentList[i];
            if (randomInt < currentWeight)
            {
                return i;
            }
        }
        return percentList.Count > 0 ? percentList.Count - 1 : -1;
    }

    public bool CanConsumeTotalCoins(int amountToCheck)
    {
        return this.totalCoinAmount >= amountToCheck;
    }

    public bool TryConsumeTotalCoins(int amountToConsume)
    {
        if (this.totalCoinAmount >= amountToConsume)
        {
            this.totalCoinAmount -= amountToConsume;
            PublishCoinData(); 
            return true;
        }
        return false;
    }

    public void SetTarget(GameObject obj)
    {
        playerObj = obj;
    }

    float FindClosestElement(float target, float[] numbers)
    {
        float closest = numbers[0];
        float smallestDifference = Math.Abs(target - closest);

        foreach (var item in numbers)
        {
            float difference = Math.Abs(target - item);

            if (difference < smallestDifference)
            {
                smallestDifference = difference;
                closest = item;
            }
        }
        return closest;
    }
}