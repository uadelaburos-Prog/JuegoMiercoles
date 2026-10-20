using UnityEngine;

// Requiere: Collider2D con Is Trigger activado y un Rigidbody2D Kinematic
public class Projectile : MonoBehaviour
{
    public static int ActiveCount { get; private set; }

    private const float MaxLifetime = 10f;

    private WeaponData weapon;
    private Vector2 direction;
    private GameObject owner;
    private float damage;
    private float traveled;
    private float age;

    public void Init(WeaponData w, Vector2 dir, GameObject ownerObject, float damageScale)
    {
        weapon = w;
        direction = dir.normalized;
        owner = ownerObject;
        damage = w.damage * damageScale;
        if (TryGetComponent(out SpriteRenderer sr)) sr.color = w.color;
    }

    private void OnEnable() { ActiveCount++; }
    private void OnDisable() { ActiveCount--; }

    private void Update()
    {
        float step = weapon.speed * Time.deltaTime;
        transform.position += (Vector3)(direction * step);
        traveled += step;
        age += Time.deltaTime;

        bool outOfRange = weapon.range > 0f && traveled >= weapon.range;
        if (outOfRange || age >= MaxLifetime) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.isTrigger) return;
        if (owner != null && (other.gameObject == owner || other.transform.IsChildOf(owner.transform))) return;

        var target = other.GetComponentInParent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(new DamageInfo
            {
                amount = damage,
                type = weapon.damageType,
                source = owner,
                direction = direction
            });
        }
        Destroy(gameObject);
    }
}
