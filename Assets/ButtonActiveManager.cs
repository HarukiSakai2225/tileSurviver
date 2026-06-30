using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonActiveManager : MonoBehaviour
{
    public static ButtonActiveManager Instance { get; private set; }
    public GameObject[] ActiveObjArray;

    void Awake()
{
    if (Instance == null)
    {
        Instance = this;
    }
    else
    {
        Destroy(gameObject);
    }
}

    public void DoSetActive(int index)
    {
        ActiveObjArray[index].SetActive(true);
    }
}
