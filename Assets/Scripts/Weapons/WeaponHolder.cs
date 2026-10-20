using System.Collections.Generic;
using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    [SerializeField] private List<WeaponData> loadout = new List<WeaponData>();
    [SerializeField] private Transform firePoint;

    private int[] ammo;
    private float[] nextFireTime;
    private int current;
    private Camera cam;
    private PlayerController movement;

    public int Count => loadout.Count;
    public int CurrentIndex => current;
    public WeaponData GetData(int i) { return loadout[i]; }
    public int GetAmmo(int i) { return ammo[i]; }

    public bool HasAnyAmmo
    {
        get
        {
            for (int i = 0; i < ammo.Length; i++) if (ammo[i] > 0) return true;
            return false;
        }
    }

    private void Awake()
    {
        cam = Camera.main;
        movement = GetComponent<PlayerController>();
        if (firePoint == null) firePoint = transform;

        ammo = new int[loadout.Count];
        nextFireTime = new float[loadout.Count];
        for (int i = 0; i < loadout.Count; i++) ammo[i] = loadout[i].maxAmmo;
    }

    private void Update()
    {
        if (Time.timeScale == 0f || loadout.Count == 0) return;

        HandleSwitch();
        if (Input.GetMouseButton(0)) TryFire();
    }

    private void HandleSwitch()
    {
        for (int i = 0; i < loadout.Count && i < 9; i++)
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) current = i;

        float scroll = Input.mouseScrollDelta.y;
        if (scroll > 0f) current = (current + 1) % loadout.Count;
        else if (scroll < 0f) current = (current - 1 + loadout.Count) % loadout.Count;
    }

    private Vector2 AimDirection()
    {
        Vector3 mouse = cam.ScreenToWorldPoint(Input.mousePosition);
        return ((Vector2)mouse - (Vector2)firePoint.position).normalized;
    }

    private void TryFire()
    {
        WeaponData w = loadout[current];
        if (ammo[current] <= 0 || Time.time < nextFireTime[current]) return;

        ammo[current]--;
        nextFireTime[current] = Time.time + w.cooldown;

        Vector2 dir = AimDirection();
        w.fireBehaviour.Fire(w, firePoint.position, dir, gameObject);

        if (w.recoil > 0f && movement != null) movement.Nudge(-dir * w.recoil);
    }
}
