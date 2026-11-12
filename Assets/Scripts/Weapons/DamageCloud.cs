using System.Collections.Generic;
using UnityEngine;

// Requiere: CircleCollider2D (Is Trigger = true) y Rigidbody2D (Kinematic)
public class DamageCloud : MonoBehaviour
{
    public static int ActiveCount { get; private set; }

    [Header("Configuracion Base")]
    [SerializeField] private float rotateSpeed = 45f;

    [Header("Sinergias (GDD 6.2)")]
    [SerializeField] private float explosionRadius = 2.5f;
    [SerializeField] private float explosionDamageMultiplier = 3f;

    private WeaponData weapon;
    private Vector2 direction;
    private GameObject owner;
    private DamageCloudGroup group;

    private float startRadius, endRadius, currentRadius;
    private float lingerTime, tickInterval, speedMultiplier;
    private float traveled, totalLifetime, age, tickTimer;
    private float baseAlpha = 0.45f;

    private bool hasStopped;
    private bool isIgnited;
    private DamageType? secondaryDamageType = null;

    // Impulso recibido por disparos de escopeta
    private Vector2 pushVelocity;

    private SpriteRenderer sr;
    public WeaponData Weapon => weapon;
    public bool IsIgnited => isIgnited;

    public void Init(
        WeaponData w, Vector2 dir, GameObject ownerObject,
        float rStart, float rEnd, float linger, float tick,
        float speedMult, DamageCloudGroup sharedGroup)
    {
        weapon = w;
        direction = dir.normalized;
        owner = ownerObject;
        startRadius = rStart;
        endRadius = rEnd;
        currentRadius = startRadius;
        lingerTime = linger;
        tickInterval = tick;
        speedMultiplier = speedMult;
        group = sharedGroup;

        float travelTime = (weapon.speed > 0f) ? (weapon.range / (weapon.speed * speedMultiplier)) : 0f;
        totalLifetime = travelTime + lingerTime;

        transform.localScale = Vector3.one * currentRadius * 2f;

        if (TryGetComponent(out sr))
        {
            Color c = w.color;
            c.a = baseAlpha;
            sr.color = c;
        }
    }

    private void OnEnable() { ActiveCount++; }
    private void OnDisable() { ActiveCount--; }

    private void Update()
    {
        age += Time.deltaTime;
        float lifeProgress = Mathf.Clamp01(age / totalLifetime);

        // 1. Expansion y rotacion
        currentRadius = Mathf.Lerp(startRadius, endRadius, lifeProgress);
        transform.localScale = Vector3.one * currentRadius * 2f;
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);

        // 2. Movimiento propio hacia adelante
        if (!hasStopped && traveled < weapon.range)
        {
            float travelRatio = Mathf.Clamp01(traveled / weapon.range);
            float currentSpeed = weapon.speed * speedMultiplier * Mathf.Lerp(1.2f, 0.25f, travelRatio);
            float step = currentSpeed * Time.deltaTime;

            if (CheckWallHit(direction, step))
            {
                StopCloud();
            }
            else
            {
                transform.position += (Vector3)(direction * step);
                traveled += step;
                if (traveled >= weapon.range) StopCloud();
            }
        }

        // 3. Movimiento extra por empuje de Escopeta (con desaceleracion suave)
        if (pushVelocity.sqrMagnitude > 0.01f)
        {
            float pushStep = pushVelocity.magnitude * Time.deltaTime;
            if (!CheckWallHit(pushVelocity.normalized, pushStep))
            {
                transform.position += (Vector3)(pushVelocity * Time.deltaTime);
            }
            else
            {
                pushVelocity = Vector2.zero; // Frena si choca con una pared
            }
            pushVelocity = Vector2.Lerp(pushVelocity, Vector2.zero, 6f * Time.deltaTime);
        }

        // 4. Control de transparencia
        if (sr != null)
        {
            Color c = sr.color;
            if (!hasStopped)
            {
                c.a = baseAlpha;
            }
            else
            {
                float lingerProgress = Mathf.Clamp01((age - (totalLifetime - lingerTime)) / lingerTime);
                c.a = Mathf.Lerp(0.75f, 0f, lingerProgress * lingerProgress);
            }
            sr.color = c;
        }

        if (age >= totalLifetime)
        {
            Destroy(gameObject);
            return;
        }

        // 5. Dano por tick
        tickTimer += Time.deltaTime;
        if (tickTimer >= tickInterval)
        {
            tickTimer -= tickInterval;
            ApplyTick();
        }
    }

    private void StopCloud()
    {
        hasStopped = true;
        traveled = weapon.range;
    }

    private bool CheckWallHit(Vector2 moveDir, float step)
    {
        var wallHits = Physics2D.CircleCastAll(transform.position, currentRadius * 0.6f, moveDir, step);
        foreach (var hit in wallHits)
        {
            if (hit.collider != null && !hit.collider.isTrigger && hit.collider.CompareTag("Wall"))
                return true;
        }
        return false;
    }

    private void ApplyTick()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, currentRadius);
        float dmgAmount = weapon.damage * tickInterval * (isIgnited ? 1.5f : 1f);

        foreach (var hit in hits)
        {
            if (hit.isTrigger) continue;
            if (owner != null && (hit.gameObject == owner || hit.transform.IsChildOf(owner.transform))) continue;

            var target = hit.GetComponentInParent<IDamageable>();
            if (target == null) continue;
            if (group != null && !group.CanDamage(target)) continue;

            target.TakeDamage(new DamageInfo
            {
                amount = dmgAmount,
                type = weapon.damageType,
                source = owner,
                direction = direction
            });

            if (secondaryDamageType.HasValue)
            {
                target.TakeDamage(new DamageInfo
                {
                    amount = dmgAmount * 0.5f,
                    type = secondaryDamageType.Value,
                    source = owner,
                    direction = direction
                });
            }
        }
    }

    // =========================================================
    // INTERACCIONES ENTRE ARMAS EN TIEMPO REAL
    // =========================================================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out DamageCloud otherCloud)) return;
        if (otherCloud == this || otherCloud.Weapon == null || this.Weapon == null) return;

        bool thisIsGas = IsGasWeapon(this.Weapon);
        bool otherIsFire = IsFireWeapon(otherCloud.Weapon);

        // Gas + Lanzallamas = Explosion
        if (thisIsGas && otherIsFire && !isIgnited)
        {
            TriggerGasExplosion();
            return;
        }

        // Insecticida + Fungicida = Superposicion
        if (thisIsGas && IsGasWeapon(otherCloud.Weapon) && this.Weapon != otherCloud.Weapon)
        {
            if (this.age > otherCloud.age)
            {
                secondaryDamageType = otherCloud.Weapon.damageType;
                if (sr != null)
                {
                    Color newColor = otherCloud.Weapon.color;
                    newColor.a = sr.color.a;
                    sr.color = newColor;
                }
            }
        }
    }

    private void TriggerGasExplosion()
    {
        isIgnited = true;
        StopCloud();

        if (sr != null) sr.color = new Color(1f, 0.45f, 0f, 0.9f);
        startRadius = explosionRadius * 0.7f;
        endRadius = explosionRadius;

        var hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        var damaged = new HashSet<IDamageable>();

        foreach (var hit in hits)
        {
            if (hit.isTrigger) continue;
            if (owner != null && (hit.gameObject == owner || hit.transform.IsChildOf(owner.transform))) continue;

            var target = hit.GetComponentInParent<IDamageable>();
            if (target == null || !damaged.Add(target)) continue;

            target.TakeDamage(new DamageInfo
            {
                amount = weapon.damage * explosionDamageMultiplier,
                type = weapon.damageType,
                source = owner,
                direction = (hit.transform.position - transform.position).normalized
            });
        }
    }

    // Llamado por cada perdigon de Escopeta que atraviesa el gas
    public void PushAndExtend(Vector2 pushDir, float pushForce, float extraTime)
    {
        // Empuja la nube en la direccion del disparo (con un tope para no mandarla al infinito)
        pushVelocity = Vector2.ClampMagnitude(pushVelocity + pushDir.normalized * pushForce, 8f);

        // Extiende la duracion (sin pasarse del tiempo de vida original)
        age = Mathf.Max(0f, age - extraTime);

        // Si ya estaba encendida por el lanzallamas, tambien reaviva el fuego (GDD 6.2)
        if (isIgnited)
        {
            endRadius = Mathf.Min(endRadius + 0.12f, explosionRadius * 1.4f);
            if (sr != null) sr.color = new Color(1f, 0.25f, 0f, 1f);
        }
    }

    public static bool IsFireWeapon(WeaponData w)
    {
        return w.damageType.ToString().ToLower().Contains("fuego") ||
               w.damageType.ToString().ToLower().Contains("fire") ||
               w.name.ToLower().Contains("lanzallamas");
    }

    public static bool IsGasWeapon(WeaponData w)
    {
        string n = w.name.ToLower();
        return n.Contains("insecticida") || n.Contains("fungicida");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isIgnited ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, currentRadius > 0f ? currentRadius : 0.5f);
    }
}