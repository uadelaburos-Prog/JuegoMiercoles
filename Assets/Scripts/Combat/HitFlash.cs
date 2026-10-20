using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class HitFlash : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private float duration = 0.12f;

    private Color original;
    private Coroutine routine;

    private void Awake()
    {
        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();
        original = sprite.color;
        GetComponent<EnemyHealth>().OnDamaged += Handle;
    }

    private void Handle(Effectiveness eff)
    {
        Color c = eff.FlashColor();
        if (c.a <= 0f) return; // Nulo: no parpadea
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Flash(c));
    }

    private IEnumerator Flash(Color c)
    {
        sprite.color = c;
        yield return new WaitForSeconds(duration);
        sprite.color = original;
        routine = null;
    }
}
