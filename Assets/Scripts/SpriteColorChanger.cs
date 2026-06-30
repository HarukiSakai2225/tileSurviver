using System.Collections;
using UnityEngine;

public class SpriteColorChanger : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] targetSprites;
    [SerializeField] private float duration = 1.0f;

    private float previousValue;
    private bool hasPreviousValue = false;

    private Color[] originalColors;

    private void Awake()
    {
        if (targetSprites == null)
        {
            originalColors = new Color[0];
            return;
        }

        originalColors = new Color[targetSprites.Length];

        for (int i = 0; i < targetSprites.Length; i++)
        {
            if (targetSprites[i] != null)
            {
                originalColors[i] = targetSprites[i].color;
            }
        }
    }

    public void CheckValueAndChangeColor(float currentValue, float unusedValue)
    {
        if (hasPreviousValue && currentValue < previousValue)
        {
            StartCoroutine(ChangeSpritesColorRedCoroutine());
        }

        previousValue = currentValue;
        hasPreviousValue = true;

        Debug.Log($"currentValue: {currentValue}, previousValue: {previousValue}");
    }

    private IEnumerator ChangeSpritesColorRedCoroutine()
    {
        if (targetSprites == null || targetSprites.Length == 0)
        {
            yield break;
        }

        for (int i = 0; i < targetSprites.Length; i++)
        {
            if (targetSprites[i] == null) continue;

            targetSprites[i].color = Color.red;
        }

        yield return new WaitForSeconds(duration);

        for (int i = 0; i < targetSprites.Length; i++)
        {
            if (targetSprites[i] == null) continue;

            targetSprites[i].color = originalColors[i];
        }
    }
}