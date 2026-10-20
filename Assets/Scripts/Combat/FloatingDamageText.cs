
using System.Collections;
using TMPro;
using UnityEngine;

public class FloatingDamageText : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private float duration = 0.7f;
    [SerializeField] private float riseDistance = 0.8f;

    private void Awake()
    {
        if (label == null)
            label = GetComponentInChildren<TMP_Text>();
    }

    public void Show(float damage, Color color)
    {
        label.text = damage.ToString("0.#");
        label.color = color;

        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        Vector3 startPosition = transform.position;
        Color startColor = label.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            transform.position = startPosition
                + Vector3.up * riseDistance * t;

            Color c = startColor;
            c.a = startColor.a * (1f - t);
            label.color = c;

            yield return null;
        }

        Destroy(gameObject);
    }
}
