using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using unityroom.Api;
using TMPro;
using System.Numerics;
using System.Drawing;

public class enemySpawnManager : MonoBehaviour, IPausable
{
    public static enemySpawnManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI countDownTimerText;
    [SerializeField] private TextMeshProUGUI TotalTimeText;
    [SerializeField] private TextMeshProUGUI countDownLabelText;
    [SerializeField] private GameObject pauseText;

    [Header("Boss Warning UI")]
    [SerializeField] private TextMeshProUGUI bossWarningText;
    [SerializeField] private float bossWarningStartTime = 60f;

    UnityEngine.Vector2[] SpawnPositions;
    int spawnPositionsIndexCount;

    bool isPaused = false;

    [SerializeField] float prepareingTime;
    [SerializeField] float intervalTime;
    [SerializeField] float phaseTime;
    [SerializeField] float spawnCount;
    [SerializeField] float spawnCountMultiplyer;
    [SerializeField] float spawnCountLimit;

    [System.Serializable]
    class SpawnObjectData
    {
        public GameObject SpawnObject;
        public int weight;
    }
    [SerializeField] SpawnObjectData[] NormalSpawnObjectDataArray;
    [SerializeField] SpawnObjectData[] BossSpawnObjectDataArray;

    float elapsedTime = 0f;
    int phaseCount;
    bool IsPhaseStart;
    float spawnInterval;
    float _spawnInterval;
    float _intervalTime;
    int spawnedCount;
    public int _spawnCount;

    public float BossSpawnChance20;
    public float BossSpawnChance25;
    bool IsBossSpawned;

    private void OnEnable()
    {
        PauseManager.Register(this);
    }

    private void OnDisable()
    {
        PauseManager.Unregister(this);
    }

    public void OnPause()
    {
        isPaused = true;
        if (pauseText != null) pauseText.SetActive(true);
    }

    public void OnResume()
    {
        isPaused = false;
        if (pauseText != null) pauseText.SetActive(false);
    }

    void Start()
    {
        SpawnPositions = GetEdgePositions();
    }
    void Update()
    {
        if (isPaused) return;

        elapsedTime += Time.deltaTime;

        prepareingTime -= Time.deltaTime;
        if (prepareingTime >= 0)
        {
            UpdatePrepareTimeDisplay(prepareingTime);
        }

            _intervalTime -= Time.deltaTime;
        if (_intervalTime >= 0)
        {
            UpdateIntervalTimeDisplay(_intervalTime);
        }
        if (prepareingTime < 0 && _intervalTime < 0 && !IsPhaseStart)
        {
            IsPhaseStart = true;
            phaseCount++;
            spawnInterval = phaseTime / spawnCount;
            spawnedCount = (int)spawnCount;
            _spawnCount = (int)spawnCount;
        }

        if (IsPhaseStart)
        {
            if (spawnedCount > 0)
            {
                UpdatePhaseTimeDisplay();
                _spawnInterval -= Time.deltaTime;
                if (_spawnInterval < 0 && _spawnCount > 0)
                {
                    if (!IsBossSpawned)
                    {
                        TrySpawnBossObject();
                        IsBossSpawned = true;
                    }
                    else
                    {
                        TrySpawnObject();
                    }
                    _spawnCount--;
                    _spawnInterval = spawnInterval;
                }
            }
            else
            {
                _intervalTime = intervalTime;
                IsPhaseStart = false;
                IsBossSpawned = false;
                spawnCount *= spawnCountMultiplyer;
            }
        }

    }

    public void decreaseSpawnedCount()
    {
        spawnedCount--;
    }

    private void UpdatePhaseTimeDisplay()
    {
        countDownLabelText.text = "";
        countDownTimerText.text = "残り" + spawnedCount + "体";
    }
    private void UpdatePrepareTimeDisplay(float time)
    {
        if (countDownTimerText == null) return;

        countDownLabelText.text = "準備時間";
        countDownLabelText.color = UnityEngine.Color.green;

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        countDownTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    private void UpdateIntervalTimeDisplay(float time)
    {
        if (countDownTimerText == null) return;

        countDownLabelText.text = "次のウェーブまで";
        countDownLabelText.color = UnityEngine.Color.green;

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        countDownTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    void SpawnObject(GameObject obj)
    {
        Instantiate(obj, GetRandomEdgePosition(), UnityEngine.Quaternion.identity, transform);
    }

    void TrySpawnObject()
    {
        if (phaseCount <= 10)
        {
            int index = DetermineWeightedIndex(NormalSpawnObjectDataArray);
            SpawnObject(NormalSpawnObjectDataArray[index].SpawnObject);
        }
        else if (phaseCount <= 20)
        {
            float value = UnityEngine.Random.Range(0f, 1f);
            if (value > BossSpawnChance20)
            {
                int index = DetermineWeightedIndex(NormalSpawnObjectDataArray);
                SpawnObject(NormalSpawnObjectDataArray[index].SpawnObject);
            }
            else
            {
                int index = DetermineWeightedIndex(BossSpawnObjectDataArray);
                SpawnObject(BossSpawnObjectDataArray[index].SpawnObject);
            }
            
        }
        else if (phaseCount <= 25)
        {
            float value = UnityEngine.Random.Range(0f, 1f);
            if (value > BossSpawnChance25)
            {
                int index = DetermineWeightedIndex(NormalSpawnObjectDataArray);
                SpawnObject(NormalSpawnObjectDataArray[index].SpawnObject);
            }
            else
            {
                int index = DetermineWeightedIndex(BossSpawnObjectDataArray);
                SpawnObject(BossSpawnObjectDataArray[index].SpawnObject);
            }
        }
        //ハードコード直すscriptableObjに
    }

    void TrySpawnBossObject()
    {
        int index;
        switch (phaseCount)
        {
            case 5:
                index = DetermineWeightedIndex(BossSpawnObjectDataArray);
                SpawnObject(BossSpawnObjectDataArray[index].SpawnObject);
                break;
                
            case 10:
                for (int i = 0; i < 2; i++)
                {
                    index = DetermineWeightedIndex(BossSpawnObjectDataArray);
                    SpawnObject(BossSpawnObjectDataArray[index].SpawnObject);
                }
                break;

            default:
                break;
        }
    }



    public void TotalTimeOnGameOver()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);

        TotalTimeText.text = "生存時間:" + string.Format("{0:00}:{1:00}", minutes, seconds);
        UnityroomApiClient.Instance.SendScore(1, elapsedTime, ScoreboardWriteMode.HighScoreDesc);

    }

    UnityEngine.Vector2 GetRandomEdgePosition()
    {
        return SpawnPositions[UnityEngine.Random.Range(0, spawnPositionsIndexCount)];
    }

    UnityEngine.Vector2[] GetEdgePositions()
    {
        if (MapGenerator.Instance != null &&
            MapGenerator.Instance.EdgeForestTiles != null &&
            MapGenerator.Instance.EdgeForestTiles.Count > 0)
        {
            spawnPositionsIndexCount = MapGenerator.Instance.EdgeForestTiles.Count;
            UnityEngine.Vector2[] spawnPositions = new UnityEngine.Vector2[spawnPositionsIndexCount];

            for (int i = 0; i < spawnPositionsIndexCount; i++)
            {
                spawnPositions[i] = MapGenerator.Instance.EdgeForestTiles[i].transform.position;
            }

            return spawnPositions;
        }

        return new UnityEngine.Vector2[0];
    }

    int DetermineWeightedIndex(SpawnObjectData[] Data)
    {
        if (Data == null || Data.Length == 0) return -1;

        int totalWeight = 0;

        foreach (var item in Data)
        {
            totalWeight += item.weight;
        }
        if (totalWeight <= 0)
        {
            Debug.LogError("Percent配列の合計が0以下です。");
            return -1;
        }

        int randomInt = UnityEngine.Random.Range(0, totalWeight + 1);
        int currentWeight = 0;

        for (int i = 0; i < Data.Length; i++)
        {
            currentWeight += Data[i].weight;

            if (randomInt < currentWeight)
            {
                return i;
            }
        }

        return -1;
    }
}