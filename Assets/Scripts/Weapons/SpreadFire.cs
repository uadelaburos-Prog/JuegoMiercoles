using UnityEngine;

// Escopeta: varios perdigones que se reparten el dano del disparo
[CreateAssetMenu(menuName = "Exterminacion/Fire/Spread")]
public class SpreadFire : FireBehaviour
{
    [SerializeField] private int pellets = 5;
    [SerializeField] private float spreadAngle = 30f;

    public override void Fire(WeaponData weapon, Vector2 origin, Vector2 direction, GameObject owner)
    {
        float baseAngle = AngleOf(direction);
        for (int i = 0; i < pellets; i++)
        {
            float t = pellets == 1 ? 0.5f : (float)i / (pellets - 1);
            float angle = baseAngle + Mathf.Lerp(-spreadAngle * 0.5f, spreadAngle * 0.5f, t);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            var go = Instantiate(weapon.projectilePrefab, origin, Quaternion.Euler(0f, 0f, angle));
            go.GetComponent<Projectile>().Init(weapon, dir, owner, 1f / pellets);
        }
    }
}
