using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphManager : MonoBehaviour
{
    float originalScaleF;
    float originalLocalPositionXF;
    [SerializeField] GameObject HPGraph;
    Vector3 newScale;

    // Start is called before the first frame update
    void Awake()
    {
        originalScaleF = HPGraph.transform.localScale.x;
        originalLocalPositionXF = HPGraph.transform.localPosition.x; 
        newScale = HPGraph.transform.localScale;
    }


    public void UpdateGraph(float currentNum, float limit)
    {
            // limitが0やマイナスにならないようにガード
        if (limit <= 0) return;

        float ratio = currentNum / limit;
        // 比率がマイナスにならないように制限
        if (ratio < 0) ratio = 0;

        // 1. スケールの更新 (localScaleは元々ローカルなので変更なし)
        newScale.x = originalScaleF * ratio;
        HPGraph.transform.localScale = newScale;

        // 2. 位置の補正
        // スケール変化によるズレの計算 (これは変更なし)
        float positionOffset = (originalScaleF - newScale.x) / 2f;

        // 現在のローカル座標を取得
        Vector3 newLocalPosition = HPGraph.transform.localPosition; 

        // Start時に保存した「元のローカルX座標」を基準に、
        // スケールのズレを補正します
        newLocalPosition.x = originalLocalPositionXF - positionOffset;
        
        // ワールド座標 (transform.position) ではなく、
        // ローカル座標 (transform.localPosition) を更新します
        HPGraph.transform.localPosition = newLocalPosition; 
    }
    
    
}
