using UnityEngine;

// Pistola
[CreateAssetMenu(menuName = "Exterminacion/Fire/Projectile")]
public class ProjectileFire : FireBehaviour
{
    public override void Fire(WeaponData weapon, Vector2 origin, Vector2 direction, GameObject owner)
    {
        var go = Instantiate(weapon.projectilePrefab, origin, Quaternion.Euler(0f, 0f, AngleOf(direction)));
        go.GetComponent<Projectile>().Init(weapon, direction, owner, 1f);
    }
}
