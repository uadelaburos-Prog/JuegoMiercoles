using UnityEngine;
using TMPro;

public class PlayerAmmoHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text ammoText;

    private void Awake()
    {
        if (ammoText == null) ammoText = GetComponent<TMP_Text>();
    }

    public void UpdateAmmo(int currentAmmo, int maxAmmo, bool isReloading = false)
    {
        if (ammoText == null) return;

        if (isReloading)
        {
            ammoText.text = "RECARGANDO...";
            ammoText.color = Color.yellow;
        }
        else
        {
            ammoText.text = $"{currentAmmo} / {maxAmmo}";
            ammoText.color = currentAmmo == 0 ? Color.red : Color.white;
        }
    }
}