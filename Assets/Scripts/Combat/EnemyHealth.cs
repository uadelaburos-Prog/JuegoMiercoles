
using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public static readonly HashSet<EnemyHealth> Alive =
        new HashSet<EnemyHealth>();

    public static event Action<EnemyHealth> EnemyKilled;

    [SerializeField] private EnemyData data;
    [SerializeField] private FloatingDamageText damageTextPrefab;

    public EnemyData Data => data;
    public float Current { get; private set; }

    public event Action<Effectiveness> OnDamaged;

    private bool dead;

    private void Awake()
    {
        Current = data.maxHealth;
    }

    private void OnEnable()
    {
        Alive.Add(this);
    }

    private void OnDisable()
    {
        Alive.Remove(this);
    }

    public void TakeDamage(DamageInfo info)
    {
        if (dead) return;

        Effectiveness eff = data.GetEffectiveness(info.type);
        float multiplier = eff.Multiplier();
        float finalDamage = info.amount * multiplier;

        // Mantiene el evento original para HitFlash.
        OnDamaged?.Invoke(eff);

        // Mostrar el daño calculado según la tabla.
        if (damageTextPrefab != null)
        {
            Color color;

            if (multiplier > 1f)
                color = Color.red;       // Débil
            else if (multiplier == 0f)
                color = Color.gray;      // Nulo
            else if (multiplier < 1f)
                color = Color.cyan;      // Resistente
            else
                color = Color.white;     // Normal

            FloatingDamageText popup = Instantiate(
                damageTextPrefab,
                transform.position + Vector3.up * 0.5f,
                Quaternion.identity
            );

            popup.Show(finalDamage, color);
        }

        if (finalDamage > 0f)
            Current -= finalDamage;

        if (Current <= 0f)
            Die();
    }

    private void Die()
    {
        dead = true;
        Alive.Remove(this);
        EnemyKilled?.Invoke(this);
        Destroy(gameObject);
    }
}
