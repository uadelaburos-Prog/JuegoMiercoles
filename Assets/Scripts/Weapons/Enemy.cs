using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody2D), typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    public enum MovementPattern
    {
        Static,         // Ej: Nido (no se mueve)
        ChaseAlways,    // Ej: Larva (va directo a por ti desde que nace)
        PatrolPoints,   // Merodea por puntos del mapa y te persigue si te detecta
        SeekCover       // Ej: Escupidor (busca cobertura y corre x1.5 si te acercas)
    }

    public enum DetectionType
    {
        None,           // No detecta (Nido o enemigos que siempre saben dónde estás)
        VisionCone,     // Ángulo de visión frontal + Raycast (paredes bloquean vista)
        FullRadius      // Radio completo 360°
    }

    [HideInInspector] public bool isMovementLocked = false;


    [Header("Identidad y Patrones")]
    [SerializeField] public string id;
    [SerializeField] private MovementPattern pattern = MovementPattern.PatrolPoints;
    [SerializeField] private DetectionType detection = DetectionType.VisionCone;
    [SerializeField] private float speed = 1f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Detección y Visión")]
    [SerializeField] private float viewRadius = 6f;
    [Range(0f, 360f)]
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private bool wallsBlockVision = true;
    [SerializeField] private LayerMask obstacleMask; // Selecciona la Layer "Wall"
    [SerializeField] private bool loseTargetWhenOutOfSight = false;
    [SerializeField] private float loseTargetDelay = 3f;

    [Header("Merodeo (PatrolPoints)")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private bool randomPatrol = false;
    [SerializeField] private float waitTimeAtPoint = 1f;

    [Header("Cobertura (SeekCover - Escupidor)")]
    [SerializeField] private Transform[] coverPoints;
    [SerializeField] private float panicRadius = 3f;
    [SerializeField] private float panicSpeedMultiplier = 1.5f;

    [Header("Separación (Boids)")]
    [SerializeField] private string[] separationTags;
    [SerializeField] private float separationRadius = 0.5f;
    [SerializeField] private float separationWeight = 1.5f;

    private Rigidbody2D rb;
    private NavMeshAgent agent;
    private Transform playerTrm;

    // Estado interno
    private bool playerDetected = false;
    private float outOfSightTimer = 0f;
    private Vector2 facingDir = Vector2.right;

    // Estado Patrulla
    private int currentPatrolIndex = 0;
    private float waitTimer = 0f;

    // Estado Cobertura
    private Transform targetCover;
    private bool inCover = false;

    private static readonly Collider2D[] hitBuffer = new Collider2D[16];

    public bool IsPlayerDetected => playerDetected;
    public bool IsInCover => inCover;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.updatePosition = false;
        agent.speed = speed;
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTrm = playerObj.transform;

        if (pattern == MovementPattern.Static)
        {
            agent.enabled = false;
            return;
        }

        if (pattern == MovementPattern.ChaseAlways)
        {
            playerDetected = true;
        }
        else if (pattern == MovementPattern.SeekCover)
        {
            FindNearestCover();
        }
        else if (pattern == MovementPattern.PatrolPoints && patrolPoints != null && patrolPoints.Length > 0)
        {
            if (randomPatrol)
                currentPatrolIndex = Random.Range(0, patrolPoints.Length);
        }
    }

    private void Update()
    {
        if (pattern == MovementPattern.Static || !agent.enabled) return;

        agent.nextPosition = rb.position;

        // 1. Evaluar detección del jugador
        if (pattern != MovementPattern.ChaseAlways)
        {
            UpdateDetection();
        }

        // 2. Decidir destino según el estado
        if (playerDetected && pattern == MovementPattern.PatrolPoints && playerTrm != null)
        {
            agent.SetDestination(playerTrm.position);
        }
        else if (pattern == MovementPattern.ChaseAlways && playerTrm != null)
        {
            agent.SetDestination(playerTrm.position);
        }
        else if (pattern == MovementPattern.PatrolPoints)
        {
            UpdatePatrol();
        }
        else if (pattern == MovementPattern.SeekCover)
        {
            UpdateCover();
        }
    }

    private void FixedUpdate()
    {
        if (rb == null || pattern == MovementPattern.Static) return;

        if (isMovementLocked) return;


        Vector2 pathDir = Vector2.zero;
        if (agent.hasPath && !(pattern == MovementPattern.SeekCover && inCover && !playerDetected))
        {
            Vector2 nextCorner = agent.steeringTarget;
            pathDir = (nextCorner - rb.position).normalized;
        }

        Vector2 separation = CalculateSeparation();
        Vector2 finalDir = pathDir + separation * separationWeight;

        if (finalDir.sqrMagnitude > 0.0001f)
        {
            finalDir.Normalize();

            // Actualizar hacia dónde mira el enemigo (para el cono de visión)
            facingDir = Vector2.Lerp(facingDir, finalDir, rotationSpeed * Time.fixedDeltaTime).normalized;
            float angle = Mathf.Atan2(facingDir.y, facingDir.x) * Mathf.Rad2Deg;
            rb.MoveRotation(angle);

            // Velocidad (incluye pánico x1.5 del Escupidor si corre hacia cobertura)
            float currentSpeed = speed;
            if (pattern == MovementPattern.SeekCover && !inCover && playerDetected)
            {
                currentSpeed *= panicSpeedMultiplier;
            }

            rb.MovePosition(rb.position + finalDir * currentSpeed * Time.fixedDeltaTime);
        }
        else if (playerDetected && playerTrm != null)
        {
            // Si está quieto (ej. en cobertura) pero te ve, gira hacia ti
            Vector2 lookToPlayer = ((Vector2)playerTrm.position - rb.position).normalized;
            facingDir = Vector2.Lerp(facingDir, lookToPlayer, rotationSpeed * Time.fixedDeltaTime).normalized;
            float angle = Mathf.Atan2(facingDir.y, facingDir.x) * Mathf.Rad2Deg;
            rb.MoveRotation(angle);
        }
    }

    private void UpdateDetection()
    {
        if (detection == DetectionType.None || playerTrm == null) return;

        bool canSeeNow = CanSeePlayer();

        if (canSeeNow)
        {
            playerDetected = true;
            outOfSightTimer = 0f;
        }
        else if (playerDetected && loseTargetWhenOutOfSight)
        {
            outOfSightTimer += Time.deltaTime;
            if (outOfSightTimer >= loseTargetDelay)
            {
                playerDetected = false;
                outOfSightTimer = 0f;
            }
        }
    }

    private bool CanSeePlayer()
    {
        Vector2 toPlayer = (Vector2)playerTrm.position - rb.position;
        float dist = toPlayer.magnitude;

        // Para el Escupidor usamos panicRadius si está buscando cobertura, sino viewRadius
        float activeRadius = viewRadius;
        if (dist > activeRadius) return false;

        // Comprobar ángulo si es Cono de Visión
        if (detection == DetectionType.VisionCone)
        {
            float angleToPlayer = Vector2.Angle(facingDir, toPlayer.normalized);
            if (angleToPlayer > viewAngle * 0.5f) return false;
        }

        // Comprobar si hay paredes bloqueando la vista
        if (wallsBlockVision)
        {
            RaycastHit2D hit = Physics2D.Raycast(rb.position, toPlayer.normalized, dist, obstacleMask);
            if (hit.collider != null) return false;
        }

        return true;
    }

    private void UpdatePatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        if (targetPoint == null) return;

        agent.SetDestination(targetPoint.position);

        if (!agent.pathPending && agent.remainingDistance <= 0.25f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtPoint)
            {
                waitTimer = 0f;
                if (randomPatrol)
                    currentPatrolIndex = Random.Range(0, patrolPoints.Length);
                else
                    currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            }
        }
    }

    private void UpdateCover()
    {
        if (targetCover == null) return;

        // Si ya está a cubierto, revisar si el jugador lo flanqueó o se acercó mucho
        if (inCover && playerTrm != null)
        {
            float distToPlayer = Vector2.Distance(rb.position, playerTrm.position);

            // Comprobar si tiene línea de visión directa (sin paredes) hacia el jugador
            Vector2 toPlayer = ((Vector2)playerTrm.position - rb.position).normalized;
            bool exposed = !Physics2D.Raycast(rb.position, toPlayer, distToPlayer, obstacleMask);

            // Si estás muy cerca (dentro del panicRadius) y expuesto, busca otro punto
            if (distToPlayer <= panicRadius && exposed)
            {
                FindAlternativeCover();
                inCover = false;
            }
            return;
        }

        agent.SetDestination(targetCover.position);

        if (!agent.pathPending && agent.remainingDistance <= 0.25f)
        {
            inCover = true;
            agent.ResetPath();
        }
    }

    private void FindAlternativeCover()
    {
        if (coverPoints == null || coverPoints.Length <= 1) return;

        Transform bestPoint = null;
        float bestScore = -Mathf.Infinity;

        foreach (Transform point in coverPoints)
        {
            if (point == null || point == targetCover) continue;

            // Prioriza el punto que esté más lejos del jugador y cerca del enemigo
            float distFromPlayer = Vector2.Distance(point.position, playerTrm.position);
            float distFromSelf = Vector2.Distance(point.position, rb.position);
            float score = distFromPlayer - (distFromSelf * 0.5f);

            if (score > bestScore)
            {
                bestScore = score;
                bestPoint = point;
            }
        }

        if (bestPoint != null)
        {
            targetCover = bestPoint;
        }
    }

    private void FindNearestCover()
    {
        if (coverPoints == null || coverPoints.Length == 0) return;

        float minDist = Mathf.Infinity;
        foreach (Transform point in coverPoints)
        {
            if (point == null) continue;
            float d = Vector2.Distance(transform.position, point.position);
            if (d < minDist)
            {
                minDist = d;
                targetCover = point;
            }
        }
    }

    [System.Obsolete]
    private Vector2 CalculateSeparation()
    {
        Vector2 separation = Vector2.zero;
        int count = Physics2D.OverlapCircleNonAlloc(rb.position, separationRadius, hitBuffer);

        for (int i = 0; i < count; i++)
        {
            Collider2D hitCollider = hitBuffer[i];
            if (hitCollider.gameObject == gameObject) continue;
            if (!MatchesAnyTag(hitCollider)) continue;

            Vector2 push = rb.position - (Vector2)hitCollider.transform.position;
            float dist = push.magnitude;

            if (dist > 0.0001f)
                separation += push / (dist * dist);
        }

        return separation;
    }

    private bool MatchesAnyTag(Collider2D col)
    {
        if (separationTags == null) return false;
        for (int i = 0; i < separationTags.Length; i++)
        {
            if (col.CompareTag(separationTags[i]))
                return true;
        }
        return false;
    }

    // Dibuja el radio y cono de visión en la vista Scene
    private void OnDrawGizmosSelected()
    {
        if (detection == DetectionType.None) return;

        float r = (pattern == MovementPattern.SeekCover) ? panicRadius : viewRadius;
        Gizmos.color = playerDetected ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, r);

        if (detection == DetectionType.VisionCone)
        {
            Vector3 forward = Application.isPlaying ? (Vector3)facingDir : transform.right;
            Vector3 leftBoundary = Quaternion.Euler(0, 0, viewAngle * 0.5f) * forward * r;
            Vector3 rightBoundary = Quaternion.Euler(0, 0, -viewAngle * 0.5f) * forward * r;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
            Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        }
    }
}