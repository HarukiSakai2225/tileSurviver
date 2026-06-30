using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LvUpButton : MonoBehaviour
{
   public int ButtonNum = -1;

    [SerializeField] GameObject ButtonObj;

    public void OnPressed()
    {
        PauseManager.Instance.Resume();
        LvUpManager.Instance.DoLvUp(ButtonNum);
        ButtonObj.SetActive(false);
        LvUpManager.Instance.TryDecideLvUp();
    }
}
