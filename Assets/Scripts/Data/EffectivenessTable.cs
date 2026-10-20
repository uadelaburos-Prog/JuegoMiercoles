using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Exterminacion/Effectiveness Table")]
public class EffectivenessTable : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public DamageType damage;
        public EnemyCategory category;
        public Effectiveness result;
    }

    [SerializeField] private Effectiveness defaultResult = Effectiveness.Neutral;
    [SerializeField] private List<Entry> entries = new List<Entry>();

    private Dictionary<(DamageType, EnemyCategory), Effectiveness> lookup;

    public Effectiveness Get(DamageType damage, EnemyCategory category)
    {
        if (lookup == null) Build();
        return lookup.TryGetValue((damage, category), out var result) ? result : defaultResult;
    }

    private void Build()
    {
        lookup = new Dictionary<(DamageType, EnemyCategory), Effectiveness>();
        foreach (var e in entries) lookup[(e.damage, e.category)] = e.result;
    }

    private void OnEnable() { lookup = null; }
    private void OnValidate() { lookup = null; }

    // Clic derecho sobre el asset en el Inspector: carga la tabla 4.5.1 del GDD
    [ContextMenu("Cargar valores del GDD")]
    private void LoadGddDefaults()
    {
        entries.Clear();
        //                                 Insecto               Carnoso                Rocoso                 Fungico
        Add(DamageType.Insecticida, Effectiveness.Debil,    Effectiveness.Resistente, Effectiveness.Nulo,       Effectiveness.Nulo);
        Add(DamageType.Fungicida,   Effectiveness.Nulo,     Effectiveness.Resistente, Effectiveness.Nulo,       Effectiveness.Debil);
        Add(DamageType.Bala,        Effectiveness.Neutral,  Effectiveness.Debil,      Effectiveness.Resistente, Effectiveness.Nulo);
        Add(DamageType.Perdigones,  Effectiveness.Neutral,  Effectiveness.Neutral,    Effectiveness.Debil,      Effectiveness.Nulo);
        Add(DamageType.Fuego,       Effectiveness.Nulo,     Effectiveness.Neutral,    Effectiveness.Nulo,       Effectiveness.Neutral);
        lookup = null;
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    private void Add(DamageType d, Effectiveness insecto, Effectiveness carnoso, Effectiveness rocoso, Effectiveness fungico)
    {
        entries.Add(new Entry { damage = d, category = EnemyCategory.Insecto, result = insecto });
        entries.Add(new Entry { damage = d, category = EnemyCategory.Carnoso, result = carnoso });
        entries.Add(new Entry { damage = d, category = EnemyCategory.Rocoso, result = rocoso });
        entries.Add(new Entry { damage = d, category = EnemyCategory.Fungico, result = fungico });
    }
}
