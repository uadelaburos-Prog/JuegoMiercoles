using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    public event Action Died;

    [HideInInspector] public bool isInvulnerable = false;
    private bool isDead = false;

    public bool IsDead => isDead;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void TakeDamage(float amount = 1f)
    {
        if (isDead || isInvulnerable) return;
        Die();
    }

    public void TakeDamage(DamageInfo info)
    {
        if (isDead || isInvulnerable) return;
        Die();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead || isInvulnerable) return;

        // Solo los proyectiles matan al tocar; los melee usan EnemyMeleeAttack
        if (other.CompareTag("EnemyProjectile"))
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        Died?.Invoke();
    }
}