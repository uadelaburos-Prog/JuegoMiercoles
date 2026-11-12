using System.Collections.Generic;
using UnityEngine;

// Requiere: Collider2D con Is Trigger activado y un Rigidbody2D Kinematic
public class Projectile : MonoBehaviour
{
    public static int ActiveCount { get; private set; }

    private const float MaxLifetime = 10f;

    [Header("Interaccion Escopeta -> Gas")]
    [SerializeField] private float shotgunPushPerPellet = 1.6f;
    [SerializeField] private float shotgunExtendPerPellet = 0.35f;

    [Header("Efectos Visuales (Opcional)")]
    [SerializeField] private ParticleSystem fireTrailParticles;

    private WeaponData weapon;
    private Vector2 direction;
    private GameObject owner;
    private float damage;
    private float traveled;
    private float age;
    private bool hasHit;
    private bool isOnFire;

    private readonly HashSet<DamageCloud> pushedClouds = new HashSet<DamageCloud>();

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
        if (hasHit) return;

        float step = weapon.speed * Time.deltaTime;

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, step);
        foreach (var hit in hits)
        {
            if (hit.collider == null || hit.collider.isTrigger) continue;
            if (IsOwner(hit.collider)) continue;

            transform.position = hit.point;
            ProcessImpact(hit.collider);
            return;
        }

        transform.position += (Vector3)(direction * step);
        traveled += step;
        age += Time.deltaTime;

        bool outOfRange = weapon.range > 0f && traveled >= weapon.range;
        if (outOfRange || age >= MaxLifetime) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit || IsOwner(other)) return;

        if (other.isTrigger && other.TryGetComponent(out DamageCloud cloud) && cloud.Weapon != null)
        {
            string weaponName = weapon.name.ToLower();

            // 1. Bala de Pistola atraviesa llamas -> Se prende fuego
            if (!isOnFire && DamageCloud.IsFireWeapon(cloud.Weapon) && weaponName.Contains("pistola"))
            {
                IgniteBullet();
            }

            // 2. Perdigones de Escopeta atraviesan gas (o mezcla encendida) -> Empujan y extienden duracion
            if (weaponName.Contains("escopeta") && DamageCloud.IsGasWeapon(cloud.Weapon))
            {
                if (pushedClouds.Add(cloud))
                {
                    cloud.PushAndExtend(direction, shotgunPushPerPellet, shotgunExtendPerPellet);
                }
            }

            return; // Atraviesa el humo/fuego sin destruirse
        }

        if (other.isTrigger) return;
        ProcessImpact(other);
    }

    private void IgniteBullet()
    {
        isOnFire = true;
        damage *= 1.5f;
        transform.localScale *= 1.6f;

        if (fireTrailParticles != null)
            fireTrailParticles.Play();

        if (TryGetComponent(out TrailRenderer tr))
        {
            tr.startWidth *= 2.2f;
            tr.time *= 1.5f;
            tr.startColor = new Color(1f, 0.8f, 0.1f, 1f);
            tr.endColor = new Color(1f, 0.15f, 0f, 0f);
        }

        StartCoroutine(FlashIgniteRoutine());
    }

    private System.Collections.IEnumerator FlashIgniteRoutine()
    {
        if (TryGetComponent(out SpriteRenderer sr))
        {
            sr.color = Color.white;
            yield return new WaitForSeconds(0.06f);
            sr.color = new Color(1f, 0.25f, 0.05f, 1f);
        }
    }

    private bool IsOwner(Collider2D col)
    {
        return owner != null && (col.gameObject == owner || col.transform.IsChildOf(owner.transform));
    }

    private void ProcessImpact(Collider2D other)
    {
        hasHit = true;

        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
            return;
        }

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