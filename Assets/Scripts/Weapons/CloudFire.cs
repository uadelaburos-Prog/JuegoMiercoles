using UnityEngine;

// Insecticida, Fungicida y Lanzallamas: una zona que avanza y hace dano por segundo
[CreateAssetMenu(menuName = "Exterminacion/Fire/Cloud")]
public class CloudFire : FireBehaviour
{
    [SerializeField] private float radius = 0.8f;
    [Tooltip("Segundos que queda quieta al llegar a su alcance maximo")]
    [SerializeField] private float lingerTime = 2f;
    [SerializeField] private float tickInterval = 0.25f;

    public override void Fire(WeaponData weapon, Vector2 origin, Vector2 direction, GameObject owner)
    {
        var go = Instantiate(weapon.projectilePrefab, origin, Quaternion.identity);
        go.GetComponent<DamageCloud>().Init(weapon, direction, owner, radius, lingerTime, tickInterval);
    }
}
