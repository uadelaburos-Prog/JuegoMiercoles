using System;
using UnityEngine;
public enum Weapons
{
    Melee, Long_Distance, Medium_Distance, Close_Combat
}
public class PlayerWeapons : MonoBehaviour
{
    [SerializeField] private MonoBehaviour[] weaponBehaviours; 

    private IWeapon[] weapons;
    private IWeapon currentWeapon;
    private int currentIndex = 0;

    private void Start()
    {
        weapons = new IWeapon[weaponBehaviours.Length];

        for (int i = 0; i < weaponBehaviours.Length; i++)
        {
            weapons[i] = weaponBehaviours[i] as IWeapon;

            if (weapons[i] == null)
            {
                Debug.LogError($"{weaponBehaviours[i].name} no implementa IWeapon");
                continue;
            }

            weaponBehaviours[i].gameObject.SetActive(false);
        }

        if (weapons.Length > 0)
            EquipWeapon(weapons[0].Type);
    }

    private void Update()
    {
        HandleWeaponSwitchInput();

        if (Input.GetKey(KeyCode.Mouse0) && currentWeapon != null && currentWeapon.CanShoot())
            currentWeapon.Shoot();
    }

    private void HandleWeaponSwitchInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) CycleWeapon(1);
        if (scroll < 0f) CycleWeapon(-1);
    }

    private void EquipWeapon(Weapons type)
    {
        int index = System.Array.FindIndex(weapons, w => w != null && w.Type == type);
        if (index == -1)
        {
            Debug.LogWarning($"No se encontró arma de tipo {type}");
            return;
        }

        currentIndex = index;
        ActivateWeapon(index);
    }

    private void CycleWeapon(int direction)
    {
        currentIndex = (currentIndex + direction + weapons.Length) % weapons.Length;
        ActivateWeapon(currentIndex);
    }

    private void ActivateWeapon(int index)
    {
        if (currentWeapon != null)
            (currentWeapon as MonoBehaviour).gameObject.SetActive(false);

        currentWeapon = weapons[index];
        (currentWeapon as MonoBehaviour).gameObject.SetActive(true);

        if (currentWeapon is Axe axe) axe.SetOwner(transform);
        // repetir para Pistol, Shotgun, etc. cuando los adaptemos
    }

}
