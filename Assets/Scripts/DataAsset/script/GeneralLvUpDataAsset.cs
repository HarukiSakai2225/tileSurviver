using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "LvUpData/General")]
public class GeneralLvUpDataAsset : ScriptableObject
{
    public string[] StatusName;
    public float[] maxRandomNum;
    public int[] StatusPercentWeight;
    public string[] LvUpString;
    public Sprite[] image;
    public string targetComponentName;
    public float[] LvUpPercentValue;

    public void LvUp(List<GameObject> list, int index, float percent)
{
    // nullになったものをリストから削除
    list.RemoveAll(item => item == null);
    
    foreach (var item in list)
    {
        ILvUpable[] components = item.GetComponents<ILvUpable>();

        foreach (var com in components)
        {
            if (com.GetType().Name == targetComponentName)
            {
                com.LvUp(index, percent);
            }
        }
    }
}
}