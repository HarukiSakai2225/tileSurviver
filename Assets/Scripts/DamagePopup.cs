using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TextMeshPro textMesh;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float sideMoveRange = 0.5f;
    [SerializeField] private float lifeTime = 1.0f;

    [Header("Scale")]
    [SerializeField] private float startScale = 1.5f;
    [SerializeField] private float endScale = 0.8f;

    private float timer;
    private Color startColor;
    private Vector3 moveDirection;

    private void Awake()
    {
        if (textMesh == null)
        {
            textMesh = GetComponent<TextMeshPro>();
        }

        startColor = textMesh.color;

        float randomX = Random.Range(-sideMoveRange, sideMoveRange);
        moveDirection = new Vector3(randomX, 1f, 0f).normalized;

        transform.localScale = Vector3.one * startScale;
    }

    public void Setup(float damage)
    {
        int roundedDamage = Mathf.RoundToInt(damage);
        textMesh.text = roundedDamage.ToString();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        if (Camera.main != null)
        {
            transform.rotation = Camera.main.transform.rotation;
        }

        float t = timer / lifeTime;

        float scale = Mathf.Lerp(startScale, endScale, t);
        transform.localScale = Vector3.one * scale;

        Color color = startColor;
        color.a = Mathf.Lerp(1f, 0f, t);
        textMesh.color = color;

        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }
}