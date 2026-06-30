using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    [SerializeField] private GameObject RespawnObj;   // マスターオブジェクト
    [SerializeField] private int RespawnCost = 100;
    [SerializeField] private GameObject ItemManager;
    [SerializeField] private GameObject deathCameraObj;

    public Vector2 RespawnPosition;

    private bool isPlayerDie = false;

    void Start()
    {
        RespawnPosition = MapGenerator.Instance.selectedPosition;
        SpawnRespawnCopy();

        // マスターは常に存在させるが、実体としては使わない
        // 必要なら最初から非アクティブにしておく
        RespawnObj.SetActive(false);
    }

    void Update()
    {
        if (isPlayerDie)
        {
            AttemptRespawn();
        }
    }

    public void AttemptRespawn()
    {
        if (LvUpManager.Instance.TryConsumeTotalCoins(RespawnCost))
        {
            SpawnRespawnCopy();
            isPlayerDie = false;
            deathCameraObj.SetActive(false);
            ItemManager.SetActive(true);
        }
        else
        {
            // コイン不足時の処理
            deathCameraObj.SetActive(true);
            ItemManager.SetActive(false);
        }
    }

    private void SpawnRespawnCopy()
    {
        // マスターを複製
        GameObject clone = Instantiate(RespawnObj);

        // 指定座標へ移動
        clone.transform.position = new Vector3(
            RespawnPosition.x,
            RespawnPosition.y,
            RespawnObj.transform.position.z
        );

        // 最後に有効化
        clone.SetActive(true);
    }

    public void OnPlayerDied()
    {
        isPlayerDie = true;
    }
}