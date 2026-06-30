using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonManager : MonoBehaviour
{
    public static ButtonManager Instance { get; private set; }

    public List<string> ButtonStringList = new List<string>();
    List<Sprite> ButtonSpriteList = new List<Sprite>();
    List<Sprite> TargetButtonSpriteList = new List<Sprite>();

    [SerializeField] TextMeshProUGUI[] buttonTexts;
    [SerializeField] Image[] buttonImages;
    [SerializeField] Image[] targetImages;

    [SerializeField] int buttonNum = 3;

    [SerializeField] GameObject ButtonObj;
    [SerializeField] LvUpButton[] buttonScript;

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip buttonCreateSE;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        for (int i = 0; i < buttonScript.Length; i++)
        {
            buttonScript[i].ButtonNum = i;
        }
    }

    public void GetString(ButtonData data)
    {
        ButtonStringList.Add(data.buttonstr);
        ButtonSpriteList.Add(data.buttonSprite);
        TargetButtonSpriteList.Add(data.targetButtonSprite);

        if (ButtonStringList.Count >= buttonNum)
        {
            CreateButton();
        }
    }

    void CreateButton()
    {
        PauseManager.Instance.Pause();
        // ▼▼▼ 修正：targetImages.Length も含める ▼▼▼
        int loopCount = Mathf.Min(
            ButtonStringList.Count, 
            buttonTexts.Length, 
            buttonImages.Length,
            targetImages.Length      // ← 追加
        );
        // ▲▲▲ 修正ここまで ▲▲▲

        for (int i = 0; i < loopCount; i++)
        {
            if (buttonTexts[i] != null)
            {
                buttonTexts[i].text = ButtonStringList[i];
            }

            if (buttonImages[i] != null)
            {
                buttonImages[i].sprite = ButtonSpriteList[i];
            }

            if (targetImages[i] != null)
            {
                targetImages[i].sprite = TargetButtonSpriteList[i];
            }
        }

        ButtonObj.SetActive(true);
        audioSource.PlayOneShot(buttonCreateSE);

        // ▼▼▼ 修正：TargetButtonSpriteList もクリア ▼▼▼
        ButtonStringList.Clear();
        ButtonSpriteList.Clear();
        TargetButtonSpriteList.Clear();  // ← 追加
        // ▲▲▲ 修正ここまで ▲▲▲
    }
}