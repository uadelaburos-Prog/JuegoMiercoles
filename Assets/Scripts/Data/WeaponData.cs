using UnityEngine;

[CreateAssetMenu(menuName = "Exterminacion/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string displayName = "Arma";
    public DamageType damageType;

    [Header("Estadisticas (1 unidad de Unity = 1 casilla)")]
    [Tooltip("Dano por disparo. En armas de nube o llama es dano por segundo.")]
    public float damage = 4f;
    [Tooltip("Casillas por segundo")]
    public float speed = 3f;
    [Tooltip("Casillas. 0 o menos significa rango ilimitado (solo para proyectiles).")]
    public float range = 2.5f;
    [Tooltip("Segundos entre disparos")]
    public float cooldown = 2f;
    public int maxAmmo = 6;
    [Tooltip("Casillas que empuja al jugador hacia atras al disparar")]
    public float recoil = 0f;

    [Header("Comportamiento de disparo")]
    public FireBehaviour fireBehaviour;
    public GameObject projectilePrefab;
    public Color color = Color.white;
}
