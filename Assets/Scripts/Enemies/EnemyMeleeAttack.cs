using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Enemy), typeof(Rigidbody2D))]
public class EnemyMeleeAttack : MonoBehaviour
{
    [Header("Parámetros de Ataque")]
    [SerializeField] private float attackRange = 1.4f;       // Distancia para iniciar el ataque
    [SerializeField] private float telegraphTime = 0.4f;     // Tiempo de aviso (0.8s para Reptante)
    [SerializeField] private float recoveryCooldown = 1f;    // Pausa después de atacar

    [Header("Embestida (Lunge hacia adelante)")]
    [SerializeField] private float lungeSpeed = 8f;          // Velocidad del salto/mordida hacia adelante
    [SerializeField] private float lungeDuration = 0.15f;    // Cuánto dura el desplazamiento
    [SerializeField] private float hitRadius = 0.55f;        // Radio del golpe en la punta
    [SerializeField] private float hitOffset = 0.45f;        // Qué tan adelante del cuerpo pega

    [Header("Feedback Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color telegraphColor = Color.red;
    [SerializeField] private GameObject warningIcon;         // Opcional: signo "!"

    private Enemy enemy;
    private Rigidbody2D rb;
    private Transform playerTrm;
    private Color originalColor;
    private bool isAttacking = false;
    private float cooldownTimer = 0f;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
        if (warningIcon != null) warningIcon.SetActive(false);
    }

    private void Start()
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

        if (isAttacking || playerTrm == null) return;

        float dist = Vector2.Distance(rb.position, playerTrm.position);
        if (enemy.IsPlayerDetected && dist <= attackRange)
        {
            StartCoroutine(PerformLungeAttack());
        }
    }

    private IEnumerator PerformLungeAttack()
    {
        isAttacking = true;
        enemy.isMovementLocked = true; // Frena la IA normal del NavMesh
        rb.linearVelocity = Vector2.zero;

        if (warningIcon != null) warningIcon.SetActive(true);

        // 1. Fase de Telegrafía: se queda quieto avisando y apunta al jugador
        Vector2 attackDir = transform.right;
        float elapsed = 0f;

        while (elapsed < telegraphTime)
        {
            if (playerTrm != null)
            {
                attackDir = ((Vector2)playerTrm.position - rb.position).normalized;
                float angle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg;
                rb.MoveRotation(angle);
            }

            if (spriteRenderer != null)
                spriteRenderer.color = Color.Lerp(originalColor, telegraphColor, elapsed / telegraphTime);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (warningIcon != null) warningIcon.SetActive(false);
        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        // 2. Fase de Embestida: se desplaza hacia adelante y chequea impacto continuo
        float lungeElapsed = 0f;
        bool alreadyHit = false;

        while (lungeElapsed < lungeDuration)
        {
            rb.MovePosition(rb.position + attackDir * lungeSpeed * Time.fixedDeltaTime);

            if (!alreadyHit)
            {
                Vector2 hitCenter = rb.position + attackDir * hitOffset;
                Collider2D[] hits = Physics2D.OverlapCircleAll(hitCenter, hitRadius);

                foreach (Collider2D hit in hits)
                {
                    if (hit.CompareTag("Player"))
                    {
                        alreadyHit = true;
                        if (PlayerHealth.Instance != null)
                            PlayerHealth.Instance.TakeDamage(1f);
                        break;
                    }
                }
            }

            lungeElapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // 3. Recuperación antes de volver a perseguir
        yield return new WaitForSeconds(recoveryCooldown);

        enemy.isMovementLocked = false;
        cooldownTimer = 0.2f;
        isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.red;
        Vector3 hitCenter = transform.position + transform.right * hitOffset;
        Gizmos.DrawWireSphere(hitCenter, hitRadius);
    }
}