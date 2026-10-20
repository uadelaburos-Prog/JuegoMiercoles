using UnityEngine;

// No necesita collider: busca objetivos con OverlapCircle cada tick
public class DamageCloud : MonoBehaviour
{
    public static int ActiveCount { get; private set; }

    [Tooltip("Capas que frenan la nube (paredes). Si queda vacio, atraviesa todo.")]
    [SerializeField] private LayerMask obstacleMask;

    private WeaponData weapon;
    private Vector2 direction;
    private GameObject owner;
    private float radius, lingerTime, tickInterval;
    private float traveled, lingerTimer, tickTimer;

    public void Init(WeaponData w, Vector2 dir, GameObject ownerObject, float r, float linger, float tick)
    {
        weapon = w;
        direction = dir.normalized;
        owner = ownerObject;
        radius = r;
        lingerTime = linger;
        tickInterval = tick;
        transform.localScale = Vector3.one * radius * 2f;
        if (TryGetComponent(out SpriteRenderer sr))
        {
            Color c = w.color;
            c.a = 0.45f;
            sr.color = c;
        }
    }

    private void OnEnable() { ActiveCount++; }
    private void OnDisable() { ActiveCount--; }

    private void Update()
    {
        if (traveled < weapon.range)
        {
            float step = weapon.speed * Time.deltaTime;
            if (Physics2D.Raycast(transform.position, direction, step, obstacleMask))
            {
                traveled = weapon.range; // choco con una pared: se queda ahi
            }
            else
            {
                transform.position += (Vector3)(direction * step);
                traveled += step;
            }
        }
        else
        {
            lingerTimer += Time.deltaTime;
            if (lingerTimer >= lingerTime)
            {
                Destroy(gameObject);
                return;
            }
        }

        tickTimer += Time.deltaTime;
        if (tickTimer >= tickInterval)
        {
            tickTimer -= tickInterval;
            ApplyTick();
        }
    }

    private void ApplyTick()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var hit in hits)
        {
            if (hit.isTrigger) continue;
            if (hit.GetComponentInParent<PlayerHealth>() != null) continue; // el jugador es inmune

            var target = hit.GetComponentInParent<IDamageable>();
            if (target == null) continue;

            target.TakeDamage(new DamageInfo
            {
                amount = weapon.damage * tickInterval,
                type = weapon.damageType,
                source = owner,
                direction = direction
            });
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius > 0f ? radius : 0.8f);
    }
}
