
using TMPro;
using UnityEngine;

public class LevelHUD : MonoBehaviour
{
    [SerializeField] private WeaponHolder weapons;
    [SerializeField] private TMP_Text weaponNameText;
    [SerializeField] private TMP_Text enemiesText;

    private void Update()
    {
        if (weapons == null || weaponNameText == null || weapons.Count == 0)
            return;
        if (enemiesText != null)
            enemiesText.text = $"Enemigos: {EnemyHealth.Alive.Count}";
        WeaponData weapon = weapons.GetData(weapons.CurrentIndex);
        weaponNameText.text = weapon.displayName;
    }
}
