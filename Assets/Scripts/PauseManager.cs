using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    bool IsGameOver;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject TileUI;

    public static PauseManager Instance { get; private set; }
    public static bool IsPaused { get; private set; }

    private static readonly HashSet<IPausable> pausables = new HashSet<IPausable>();

    public static void Register(IPausable pausable)
    {
        if (pausable == null) return;
        pausables.Add(pausable);
    }

    public static void Unregister(IPausable pausable)
    {
        if (pausable == null) return;
        pausables.Remove(pausable);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate PauseManager destroyed: " + gameObject.name);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        IsPaused = false;
        pausables.Clear();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (LvUpManager.Instance != null && LvUpManager.Instance.OnLvUp && !IsGameOver)
        {
            Debug.Log("Pause blocked: LvUpManager.OnLvUp is true");
            return;
        }
        if (!IsGameOver)
        {
            if (IsPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
        }
    }

    public void Pause()
    {
        if (IsPaused) return;

        IsPaused = true;
        Debug.Log("Pause");

        foreach (var pausable in new List<IPausable>(pausables))
        {
            pausable?.OnPause();
        }
    }

    public void Resume()
    {
        if (!IsPaused) return;

        IsPaused = false;
        Debug.Log("Resume");

        foreach (var pausable in new List<IPausable>(pausables))
        {
            pausable?.OnResume();
        }
    }

    public void GameOver()
    {
        Pause();
        IsGameOver = true;

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
            TileUI.SetActive(false);
        }
    }
}