using UnityEngine;
using UnityEngine.SceneManagement;

public class ElevatorExit : MonoBehaviour
{
    [Header("Siguiente Nivel")]
    [Tooltip("Deja vacío para cargar la siguiente escena en Build Settings")]
    [SerializeField] private string nextSceneName;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color lockedColor = Color.red;
    [SerializeField] private Color unlockedColor = Color.green;

    private bool isUnlocked = false;

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Usa directamente el HashSet de tu clase EnemyHealth
        bool allDead = EnemyHealth.Alive.Count == 0;

        if (allDead != isUnlocked)
        {
            isUnlocked = allDead;
            if (spriteRenderer != null)
                spriteRenderer.color = isUnlocked ? unlockedColor : lockedColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isUnlocked) return;

        if (other.CompareTag("Player"))
        {
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                int nextIndex = (SceneManager.GetActiveScene().buildIndex + 1) % SceneManager.sceneCountInBuildSettings;
                SceneManager.LoadScene(nextIndex);
            }
        }
    }
}