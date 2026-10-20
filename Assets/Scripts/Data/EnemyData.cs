using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Exterminacion/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Serializable]
    public struct Override
    {
        public DamageType damage;
        public Effectiveness result;
    }

    public string displayName = "Enemigo";
    public EnemyCategory category;
    public float maxHealth = 15f;
    [Tooltip("Casillas por segundo")]
    public float moveSpeed = 1f;

    [Header("Efectividad")]
    public EffectivenessTable table;
    [Tooltip("Excepciones puntuales. Tienen prioridad sobre la tabla por categoria.")]
    public List<Override> overrides = new List<Override>();

    public Effectiveness GetEffectiveness(DamageType type)
    {
        foreach (var o in overrides)
            if (o.damage == type) return o.result;

        return table != null ? table.Get(type, category) : Effectiveness.Neutral;
    }
}
