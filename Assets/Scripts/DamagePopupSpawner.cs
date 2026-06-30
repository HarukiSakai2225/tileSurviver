using UnityEngine;

public class DamagePopupSpawner : MonoBehaviour
{
    [SerializeField] private DamagePopup damagePopupPrefab;

    [Header("Spawn Position")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, 0f);

    public void ShowDamage(float damage)
    {
        if (damage <= 0f)
        {
            Debug.Log($"DamagePopupは生成されませんでした。Damageが0以下です。Damage: {damage}");
            return;
        }

        if (damagePopupPrefab == null)
        {
            Debug.LogError("DamagePopup Prefabが設定されていません。InspectorでDamage Popup PrefabにPrefabを入れてください。");
            return;
        }

        Vector3 spawnPosition = transform.position + offset;

        DamagePopup popup = Instantiate(
            damagePopupPrefab,
            spawnPosition,
            Quaternion.identity
        );

        int roundedDamage = Mathf.RoundToInt(damage);
        Debug.Log($"DamagePopupを生成しました。Damage: {roundedDamage}, RawDamage: {damage}, Position: {spawnPosition}");

        popup.Setup(damage);
    }
}