using UnityEngine;

// IA minima: perseguir al jugador. Sirve para probar la tabla de tipos con las cuatro categorias.
// Rigidbody2D: Gravity Scale 0 y Freeze Rotation activado
[RequireComponent(typeof(EnemyHealth), typeof(Rigidbody2D))]
public class EnemyChaser : MonoBehaviour
{
    private EnemyHealth health;
    private Rigidbody2D rb;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        var player = PlayerHealth.Instance;
        if (player == null || player.IsDead) return;

        Vector2 toPlayer = (Vector2)player.transform.position - rb.position;
        rb.MovePosition(rb.position + toPlayer.normalized * health.Data.moveSpeed * Time.fixedDeltaTime);
    }

    // Contacto con el jugador: muerte instantanea
    private void OnCollisionStay2D(Collision2D collision)
    {
        var player = collision.collider.GetComponentInParent<PlayerHealth>();
        if (player == null) return;

        player.TakeDamage(new DamageInfo { amount = 1f, source = gameObject });
    }
}
