using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyDifficultySettings",
    menuName = "Game/Enemy Difficulty Settings"
)]
public class EnemyDifficultySettings : ScriptableObject
{
    [Header("Basic Settings")]
    public GameObject[] spawnObjectArray;
    public int[] enemySpawnPercent;
    public float spawnRate = 2.0f;
    public float spawnRateLimit = 0.5f;
    public float spawnRateMultiplier = 1.05f;
    public float spawnRateStepTimer = 10f;

    [Header("Boss Warning UI")]
    public float bossWarningStartTime = 60f;

    [Header("Phase Settings")]
    public float countDownReadyTime = 30f;

    [Header("Multiple Summon Settings")]
    public GameObject multipleSummonSpawnObj;
    public float multipleSummonInterval = 60f;
    public int multipleSummonMinNum = 3;
    public int multipleSummonMaxNum = 6;
    public float multipleSummonNumMultiplier = 1.2f;
    public float positionRandomRange = 1.5f;

    [Header("Boss Settings")]
    public GameObject[] bossObjArray;
    public int[] bossPercentWeightArray;
    public float bossSummonInterval = 120f;
    public int bossSummonMinNum = 1;
    public int bossSummonMaxNum = 1;
    public float bossSummonNumMultiplier = 1.2f;
    public float bossPositionRandomRange = 2.5f;
}