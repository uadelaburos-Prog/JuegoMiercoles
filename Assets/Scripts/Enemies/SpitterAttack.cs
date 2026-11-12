using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class SpitterAttack : MonoBehaviour
{
    [Header("Disparo del Escupidor")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float chargeTime = 3f;
    [SerializeField] private float cooldownBetweenShots = 2f;

    [Header("Feedback Visual de Carga")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color chargeColor = Color.magenta;

    private Enemy enemy;
    private Transform playerTrm;
    private Color originalColor;
    private bool isAttacking = false;
    private float cooldownTimer = 0f;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    private void Start()
    {
        FindPlayer();
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTrm = playerObj.transform;
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        if (playerTrm == null) FindPlayer();

        if (enemy.IsPlayerDetected && !isAttacking && playerTrm != null)
        {
            StartCoroutine(ChargeAndShoot());
        }
    }

    private IEnumerator ChargeAndShoot()
    {
        isAttacking = true;

        float elapsed = 0f;
        while (elapsed < chargeTime)
        {
            if (spriteRenderer != null)
                spriteRenderer.color = Color.Lerp(originalColor, chargeColor, elapsed / chargeTime);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        if (projectilePrefab == null)
        {
            Debug.LogWarning("SpitterAttack: ¡Falta asignar Projectile Prefab en el Inspector!");
        }
        else if (playerTrm == null)
        {
            Debug.LogWarning("SpitterAttack: ¡No se encontró ningún objeto con el Tag 'Player'!");
        }
        else
        {
            Vector3 origin = firePoint != null ? firePoint.position : transform.position;
            origin.z = 0f; // Asegura que nazca en el plano 2D visible

            Vector2 dir = ((Vector2)playerTrm.position - (Vector2)origin).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            Instantiate(projectilePrefab, origin, Quaternion.Euler(0, 0, angle));
        }

        cooldownTimer = cooldownBetweenShots;
        isAttacking = false;
    }
}