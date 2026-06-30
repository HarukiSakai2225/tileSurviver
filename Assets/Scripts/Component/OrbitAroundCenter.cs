using UnityEngine;
using System.Collections.Generic;

public class OrbitAroundCenter : MonoBehaviour, ILvUpable
{
    [Header("設定")]
    public Transform centerTransform;              // 中心となるオブジェクト
    public GameObject orbitingPrefab;              // 生成するプレハブ
    public int objCount = 1; 
    int currentObjCount;                      // 生成する数
    public float radius = 3f;                      // 円の半径
    public float orbitSpeed = 90f;                 // 回転速度（度/秒）
    public bool clockwise = false;                 // 時計回りかどうか

    [Header("開始角度")]
    public float startAngle = 0f;                  // 開始時の角度（度）

    private float currentAngle;
    private List<GameObject> orbitingObjects = new List<GameObject>();

    public void LvUp(int num, float percent)
    {
        switch (num)
        {
            case 0: objCount += 1; break;
            default: break;
        }
    }

    void Start()
    {
        currentAngle = startAngle;
        GenerateObjects();
        if(centerTransform == null)
        {
            centerTransform = transform;
        }
    }

    void Update()
    {
        if (centerTransform == null || orbitingObjects.Count == 0) return;

        // 角度を更新
        if (clockwise)
        {
            currentAngle -= orbitSpeed * Time.deltaTime;
        }
        else
        {
            currentAngle += orbitSpeed * Time.deltaTime;
        }

        // 角度を0〜360に正規化
        if (currentAngle >= 360f) currentAngle -= 360f;
        if (currentAngle < 0f) currentAngle += 360f;

        // 各オブジェクトの間隔を計算
        float angleStep = 360f / orbitingObjects.Count;

        // 各オブジェクトを配置
        for (int i = 0; i < orbitingObjects.Count; i++)
        {
            if (orbitingObjects[i] == null) continue;

            // このオブジェクトの角度を計算（等間隔に配置）
            float objectAngle = currentAngle + (angleStep * i);

            // 角度をラジアンに変換
            float angleRad = objectAngle * Mathf.Deg2Rad;

            // 円周上の位置を計算
            Vector2 offset = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * radius;
            Vector2 targetPosition = (Vector2)centerTransform.position + offset;

            // オブジェクトを移動
            orbitingObjects[i].transform.position = targetPosition;

            // オブジェクトを中心から外側に向ける
            orbitingObjects[i].transform.rotation = Quaternion.Euler(0, 0, objectAngle - 90f);
        }

        if(currentObjCount != objCount)
        {
            GenerateObjects();
            currentObjCount = objCount;
        }
    }

    /// <summary>
    /// オブジェクトを生成
    /// </summary>
    public void GenerateObjects()
    {
        ClearObjects();

        if (orbitingPrefab == null || objCount <= 0) return;

        for (int i = 0; i < objCount; i++)
        {
            GameObject obj = Instantiate(orbitingPrefab);
            orbitingObjects.Add(obj);
        }
    }

    /// <summary>
    /// 生成したオブジェクトを全て削除
    /// </summary>
    public void ClearObjects()
    {
        foreach (var obj in orbitingObjects)
        {
            if (obj != null)
            {
                if (Application.isPlaying)
                    Destroy(obj);
                else
                    DestroyImmediate(obj);
            }
        }
        orbitingObjects.Clear();
    }

    void OnDestroy()
    {
        ClearObjects();
    }
}