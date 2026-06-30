using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneChangeButton : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private Button targetButton;

    [Header("Scene")]
    [SerializeField] private string sceneName;

    [SerializeField] private EnemyDifficultySettings difficultySettings;

    private void OnEnable()
    {
        if (targetButton != null)
        {
            targetButton.onClick.AddListener(ChangeScene);
        }
    }

    private void OnDisable()
    {
        if (targetButton != null)
        {
            targetButton.onClick.RemoveListener(ChangeScene);
        }
    }

    private void ChangeScene()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("切り替え先のシーン名が設定されていません。");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}