using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "UnlockComponent")]
public class UnlockComponentAsset : GeneralLvUpDataAsset
{
    public string componentName;
    
    public void UnlockComponent(List<GameObject> list)
    {
        Debug.Log($"[UnlockComponent] 開始 - コンポーネント名: {targetComponentName}, 対象リスト数: {list?.Count ?? 0}");
        
        if (list == null || list.Count == 0)
        {
            Debug.LogWarning("[UnlockComponent] 対象リストが空またはnullです");
            return;
        }
        
        int totalUnlocked = 0;
        
        foreach (var item in list)
        {
            if (item == null)
            {
                Debug.LogWarning("[UnlockComponent] リスト内にnullのGameObjectがあります");
                continue;
            }
            
            Debug.Log($"[UnlockComponent] 処理中: {item.name}");
            
            ILvUpable[] components = item.GetComponents<ILvUpable>();
            Debug.Log($"[UnlockComponent] {item.name} で見つかったILvUpable数: {components.Length}");
            
            foreach (var com in components)
            {
                string typeName = com.GetType().Name;
                Debug.Log($"[UnlockComponent] 検査中: {typeName} (目標: {targetComponentName})");
                
                if (typeName == targetComponentName)
                {
                    if (com is MonoBehaviour mb)
                    {
                        bool wasPreviouslyEnabled = mb.enabled;
                        mb.enabled = true;
                        totalUnlocked++;
                        Debug.Log($"[UnlockComponent] ✓ 有効化成功: {item.name}.{typeName} (以前の状態: {wasPreviouslyEnabled})");
                    }
                    else
                    {
                        Debug.LogWarning($"[UnlockComponent] {typeName} はMonoBehaviourではありません");
                    }
                }
            }
        }
        
        Debug.Log($"[UnlockComponent] 完了 - 合計 {totalUnlocked} 個のコンポーネントを有効化しました");
        
        if (totalUnlocked == 0)
        {
            Debug.LogWarning($"[UnlockComponent] 警告: {targetComponentName} が1つも見つかりませんでした");
        }
    }
}