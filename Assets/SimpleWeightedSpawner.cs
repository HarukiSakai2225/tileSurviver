using UnityEngine;

public class SimpleWeightedSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] spawnObjectArray;
    [SerializeField] private int[] spawnPercentArray;
    [SerializeField] private float spawnInterval = 2.0f;

    [Header("Target Settings")]
    [SerializeField] private Vector2 targetPosition;

    [Header("Debug")]
    [SerializeField] private bool showSpawnLog = true;

    private float spawnTimer = 0f;

    private void Update()
    {
        if (spawnInterval <= 0f) return;

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnObject();
        }
    }

    private void SpawnObject()
    {
        if (spawnObjectArray == null || spawnObjectArray.Length == 0)
        {
            Debug.LogWarning("Spawn Object Array が空です。");
            return;
        }

        if (spawnPercentArray == null || spawnPercentArray.Length == 0)
        {
            Debug.LogWarning("Spawn Percent Array が空です。");
            return;
        }

        if (spawnObjectArray.Length != spawnPercentArray.Length)
        {
            Debug.LogError("Spawn Object Array と Spawn Percent Array の数が一致していません。");
            return;
        }

        int index = DetermineWeightedIndex(spawnPercentArray);

        GameObject prefab = spawnObjectArray[index];

        if (prefab == null)
        {
            Debug.LogWarning($"Spawn Object Array の {index} 番目が None です。");
            return;
        }

        GameObject spawnedObject = Instantiate(
            prefab,
            transform.position,
            Quaternion.identity
        );

        SSSimpleTargetSeeker seeker = spawnedObject.GetComponent<SSSimpleTargetSeeker>();

        if (seeker != null)
        {
            seeker.SetTargetPosition(targetPosition);
        }
        else
        {
            Debug.LogWarning($"{spawnedObject.name} に SSSimpleTargetSeeker が付いていません。目的地を設定できません。");
        }

        if (showSpawnLog)
        {
            Debug.Log($"スポーンしました: {prefab.name}, Spawn: {transform.position}, Target: {targetPosition}");
        }
    }

    private int DetermineWeightedIndex(int[] percentArray)
    {
        int totalWeight = 0;

        for (int i = 0; i < percentArray.Length; i++)
        {
            if (percentArray[i] > 0)
            {
                totalWeight += percentArray[i];
            }
        }

        if (totalWeight <= 0)
        {
            Debug.LogError("Spawn Percent Array の合計が0以下です。");
            return 0;
        }

        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        for (int i = 0; i < percentArray.Length; i++)
        {
            if (percentArray[i] <= 0)
            {
                continue;
            }

            currentWeight += percentArray[i];

            if (randomValue < currentWeight)
            {
                return i;
            }
        }

        return 0;
    }
}