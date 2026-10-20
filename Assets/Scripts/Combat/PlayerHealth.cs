using System;
using UnityEngine;

// Un solo golpe mata al jugador (GDD 4.2)
public class PlayerHealth : MonoBehaviour, IDamageable
{
    public static PlayerHealth Instance { get; private set; }

    public event Action Died;
    public bool IsDead { get; private set; }

    private void Awake() { Instance = this; }

    public void TakeDamage(DamageInfo info)
    {
        if (IsDead) return;
        IsDead = true;
        Died?.Invoke();
    }
}
