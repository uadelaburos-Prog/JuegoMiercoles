using UnityEngine;

// Rigidbody2D: Gravity Scale 0 y Freeze Rotation activado
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3.5f;

    private Rigidbody2D rb;
    private Camera cam;
    private PlayerDash playerDash;
    private Vector2 input;
    private Vector2 pendingNudge;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        playerDash = GetComponent<PlayerDash>();
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        // El sprite debe mirar hacia la derecha (eje X) para que la rotacion coincida con el mouse
        Vector3 mouse = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 aim = (Vector2)mouse - (Vector2)transform.position;
        float angle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void FixedUpdate()
    {
        // Si está haciendo Dash, cedemos el control del movimiento a PlayerDash
        if (playerDash != null && playerDash.IsDashing)
        {
            pendingNudge = Vector2.zero;
            return;
        }

        rb.MovePosition(rb.position + input * moveSpeed * Time.fixedDeltaTime + pendingNudge);
        pendingNudge = Vector2.zero;
    }

    // Retroceso de las armas
    public void Nudge(Vector2 delta) { pendingNudge += delta; }
}