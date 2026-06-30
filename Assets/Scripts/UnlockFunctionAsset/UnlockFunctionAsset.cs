using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "UnlockFunction")]
public class UnlockFunctionAsset : ScriptableObject
{
    public int SetActiveIndex;
    public void UnlockFunction()
    {
        ButtonActiveManager.Instance.DoSetActive(SetActiveIndex);
    }
}
