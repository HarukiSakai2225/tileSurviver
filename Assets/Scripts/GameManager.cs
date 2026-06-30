using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public UnityEvent<GameObject> OnPlayerSpawn = new UnityEvent<GameObject>();
    public UnityEvent<int> OnCoinCollected = new UnityEvent<int>();
    public UnityEvent OnPlayerDied = new UnityEvent();
    public UnityEvent<int> OnLvUpButtonPressed = new UnityEvent<int>();
    public UnityEvent<int> OnLvUp = new UnityEvent<int>();

    public UnityEvent<ButtonData> OnDecidedLvUpButton = new UnityEvent<ButtonData>();
    
    public UnityEvent OnGameOver = new UnityEvent();


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
}