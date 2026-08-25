using UnityEngine;

public interface IWeapon
{
    Weapons Type { get; }
    void Shoot();

    bool CanShoot();

    void DoDamage(RaycastHit2D hit);

    void CoolDown();
}
