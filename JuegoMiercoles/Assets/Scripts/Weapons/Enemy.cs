using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] public string id;
    [SerializeField] private float speed;
    [SerializeField] private Transform playerTrm;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private string[] names;

    private static readonly Collider2D[] hitBuffer = new Collider2D[16];

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    [System.Obsolete]
    private void FixedUpdate()
    {
        if (playerTrm == null || rb == null) return;

        Vector2 separation = Vector2.zero;
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, 0.5f, hitBuffer);

        for (int i = 0; i < count; i++)
        {
            Collider2D hitcollider = hitBuffer[i];
            if (hitcollider.gameObject == gameObject) continue;
            if (!MatchesAnyTag(hitcollider)) continue;

            Vector2 push = (Vector2)transform.position - (Vector2)hitcollider.transform.position;
            float dist = push.magnitude;

            if (dist > 0.0001f)
                separation += push / (dist * dist); // equivalente a push.normalized / dist, sin normalizar dos veces
        }

        Vector2 toPlayer = ((Vector2)playerTrm.position - rb.position).normalized;
        Vector2 finalDir = (toPlayer + separation * 0.5f).normalized;

        rb.MovePosition(rb.position + finalDir * speed * Time.fixedDeltaTime);
    }

    private bool MatchesAnyTag(Collider2D col)
    {
        for (int i = 0; i < names.Length; i++)
        {
            if (col.CompareTag(names[i]))
                return true;
        }
        return false;
    }
}
