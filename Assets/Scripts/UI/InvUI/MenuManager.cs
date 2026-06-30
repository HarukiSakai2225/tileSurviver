using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] GameObject[] MenuButtonGO;
    private MenuButton[] menuButtonScripts; // ボタンのスクリプトを保持する配列
    private MenuButton currentHoveredButton; // 現在ホバーされているボタン
    MenuButton ClickedButton;

    void Start()
    {
        // MenuButtonGO 配列の要素数を取得
        int buttonCount = MenuButtonGO.Length;
        
        // menuButtonScripts 配列を正しいサイズで初期化
        menuButtonScripts = new MenuButton[buttonCount];

        // forループを使って各GameObjectからMenuButtonスクリプトを取得し、配列に格納
        for (int i = 0; i < buttonCount; i++)
        {
            menuButtonScripts[i] = MenuButtonGO[i].GetComponent<MenuButton>();
        }
    }

    void Update()
    {

        int layerMask = LayerMask.GetMask("MenuUI");
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, layerMask);


        if (hit.collider != null)
        {
            // ヒットしたオブジェクトからMenuButtonスクリプトを取得
            MenuButton hitButton = hit.collider.GetComponent<MenuButton>();

            // もし新しくホバーしたボタンが、直前までホバーしていたボタンと違うなら
            if (hitButton != null && hitButton != currentHoveredButton)
            {
                // 直前のボタンのホバー状態を解除
                if (currentHoveredButton != null)
                {
                    currentHoveredButton.isHovering = false;
                }
                
                // 新しいボタンをホバー状態にする
                currentHoveredButton = hitButton;
                currentHoveredButton.isHovering = true;
            }
            if(Input.GetMouseButtonDown(0))
                {
                    if(ClickedButton != null)
                    ClickedButton.isClicked = false;

                    currentHoveredButton.isClicked = true;
                    ClickedButton = currentHoveredButton;
                    
                }
        }
        else
        {
            // 何もヒットしなかった場合、ホバー状態を解除
            if (currentHoveredButton != null)
            {
                currentHoveredButton.isHovering = false;
                currentHoveredButton = null;
            }
        }
    }
}