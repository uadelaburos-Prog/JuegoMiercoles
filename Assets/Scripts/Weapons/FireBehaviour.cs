using UnityEngine;

// Strategy: cada tipo de disparo es un asset intercambiable
public abstract class FireBehaviour : ScriptableObject
{
    public abstract void Fire(WeaponData weapon, Vector2 origin, Vector2 direction, GameObject owner);

    protected static float AngleOf(Vector2 dir)
    {
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }
}
