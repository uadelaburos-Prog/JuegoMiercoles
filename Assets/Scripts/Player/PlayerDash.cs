using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerHealth))]
public class PlayerDash : MonoBehaviour
{
    [Header("Configuración de Dash")]
    [SerializeField] private float dashSpeed = 16f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.8f;

    private Rigidbody2D rb;
    private PlayerHealth playerHealth;
    private bool isDashing = false;
    private float cooldownTimer = 0f;

    public bool IsDashing => isDashing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        bool dashPressed = false;

        // 1. Soporta New Input System
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) dashPressed = true;
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame) dashPressed = true;

        // 2. Soporta Legacy Input Manager (por si el proyecto usa el sistema clásico)
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(1)) dashPressed = true;

        if (dashPressed && !isDashing && cooldownTimer <= 0f)
        {
            StartCoroutine(DoDash());
        }
    }

    private IEnumerator DoDash()
    {
        isDashing = true;
        playerHealth.isInvulnerable = true;
        cooldownTimer = dashCooldown;

        // Leer dirección de movimiento (WASD/Flechas); si estás quieto, impulsa hacia donde apunta el jugador
        Vector2 inputDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        Vector2 dashDir = inputDir.sqrMagnitude > 0.01f ? inputDir : (Vector2)transform.right;

        int playerLayer = gameObject.layer;
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1)
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            // MovePosition funciona con Dynamic y Kinematic sin depender de linearVelocity
            rb.MovePosition(rb.position + dashDir * dashSpeed * Time.fixedDeltaTime);
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        if (enemyLayer != -1)
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);

        rb.linearVelocity = Vector2.zero;
        playerHealth.isInvulnerable = false;
        isDashing = false;
    }
}