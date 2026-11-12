using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMeleeAttack : MonoBehaviour
{
    [Header("Configuración de Golpe Melee")]
    [SerializeField] private float hitRadius = 0.8f;          // Tamaño del golpe
    [SerializeField] private float hitOffset = 0.7f;          // Distancia frente al jugador
    [SerializeField] private float knockbackForce = 10f;      // Empuje hacia atrás
    [SerializeField] private float stunDuration = 0.4f;       // Breve aturdimiento al empujar
    [SerializeField] private float attackCooldown = 0.5f;

    [Header("Feedback Visual (Opcional)")]
    [SerializeField] private GameObject slashVisual;          // Un sprite/efecto hijo frente al jugador
    [SerializeField] private float visualDuration = 0.12f;

    private float cooldownTimer = 0f;

    private void Awake()
    {
        if (slashVisual != null) slashVisual.SetActive(false);
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        // Tecla F, V o Click Central (rueda del mouse)
        bool meleePressed = (Keyboard.current != null && (Keyboard.current.fKey.wasPressedThisFrame || Keyboard.current.vKey.wasPressedThisFrame))
                         || (Mouse.current != null && Mouse.current.middleButton.wasPressedThisFrame);

        if (meleePressed)
        {
            PerformMelee();
        }
    }

    private void PerformMelee()
    {
        cooldownTimer = attackCooldown;

        if (slashVisual != null)
            StartCoroutine(ShowSlashVisual());

        Vector2 attackDir = transform.right;
        Vector2 hitCenter = (Vector2)transform.position + attackDir * hitOffset;

        Collider2D[] hits = Physics2D.OverlapCircleAll(hitCenter, hitRadius);

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            // 1. Empujar hacia atrás (Knockback)
            Rigidbody2D enemyRb = hit.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                enemyRb.AddForce(attackDir * knockbackForce, ForceMode2D.Impulse);
            }

            // 2. Breve aturdimiento si tiene Enemy.cs
            Enemy enemyScript = hit.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                StartCoroutine(BriefStun(enemyScript));
            }

            // 3. Hacer daño si tiene EnemyHealth (ajusta la firma según tu EnemyHealth.cs)
            // Si quieres que mate larvas o haga daño físico, descomenta y adapta la línea de abajo:
            // hit.GetComponent<EnemyHealth>()?.TakeDamage(1f);
        }
    }

    private IEnumerator BriefStun(Enemy enemy)
    {
        enemy.isMovementLocked = true;
        yield return new WaitForSeconds(stunDuration);
        if (enemy != null)
            enemy.isMovementLocked = false;
    }

    private IEnumerator ShowSlashVisual()
    {
        slashVisual.SetActive(true);
        yield return new WaitForSeconds(visualDuration);
        if (slashVisual != null)
            slashVisual.SetActive(false);
    }

    // Dibuja en rojo en la vista Scene el área del golpe melee
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 hitCenter = transform.position + transform.right * hitOffset;
        Gizmos.DrawWireSphere(hitCenter, hitRadius);
    }
}