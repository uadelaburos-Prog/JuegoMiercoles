using System.Collections.Generic;
using UnityEngine;

// Insecticida, Fungicida y Lanzallamas: rafaga de nubes en cono que se expanden y desvanecen
[CreateAssetMenu(menuName = "Exterminacion/Fire/Cloud")]
public class CloudFire : FireBehaviour
{
    [Header("Forma del chorro")]
    [SerializeField] private int puffCount = 4;
    [SerializeField] private float spreadAngle = 22f;
    [SerializeField] private float speedVariance = 0.2f;

    [Header("Comportamiento de cada nube")]
    [SerializeField] private float startRadius = 0.35f;
    [SerializeField] private float endRadius = 1.1f;
    [Tooltip("Segundos que flota antes de desaparecer del todo")]
    [SerializeField] private float lingerTime = 1.5f;
    [SerializeField] private float tickInterval = 0.25f;

    public override void Fire(WeaponData weapon, Vector2 origin, Vector2 direction, GameObject owner)
    {
        float baseAngle = AngleOf(direction);
        // Grupo compartido para que este disparo no haga dano multiple si las nubes se superponen
        var sharedGroup = new DamageCloudGroup(tickInterval);

        for (int i = 0; i < puffCount; i++)
        {
            float randomOffset = Random.Range(-spreadAngle * 0.5f, spreadAngle * 0.5f);
            float angle = baseAngle + randomOffset;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            float speedMult = 1f + Random.Range(-speedVariance, speedVariance);

            var go = Instantiate(weapon.projectilePrefab, origin, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));
            go.GetComponent<DamageCloud>().Init(
                weapon, dir, owner,
                startRadius, endRadius,
                lingerTime, tickInterval,
                speedMult, sharedGroup
            );
        }
    }
}

// Controla que un enemigo no reciba dano de 4 nubes al mismo tiempo en el mismo instante
public class DamageCloudGroup
{
    private readonly Dictionary<IDamageable, float> lastHitTime = new Dictionary<IDamageable, float>();
    private readonly float cooldown;

    public DamageCloudGroup(float tickInterval)
    {
        cooldown = tickInterval * 0.9f; // Pequeño margen para evitar desincronizacion de frames
    }

    public bool CanDamage(IDamageable target)
    {
        if (lastHitTime.TryGetValue(target, out float lastTime))
        {
            if (Time.time - lastTime < cooldown) return false;
        }
        lastHitTime[target] = Time.time;
        return true;
    }
}