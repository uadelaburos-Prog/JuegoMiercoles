using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum LevelState { Playing, Won, Lost }

public class LevelManager : MonoBehaviour
{
    [SerializeField] private PlayerHealth player;
    [SerializeField] private WeaponHolder weapons;
    [Tooltip("Menor a 0: muestra el panel de derrota. Mayor o igual a 0: reintenta solo tras esos segundos.")]
    [SerializeField] private float autoRetryDelay = -1f;
    [Tooltip("Nombre de la escena siguiente (debe estar en Build Settings). Vacio: no hay siguiente.")]
    [SerializeField] private string nextSceneName;

    [SerializeField] private GameObject defeatText;
    [SerializeField] private float retryDelay = 3f;

    [SerializeField] private GameObject elevator;
    public LevelState State { get; private set; } = LevelState.Playing;
    public string LoseReason { get; private set; }
    public bool HasNextLevel => !string.IsNullOrEmpty(nextSceneName);
    public event Action<LevelState> StateChanged;

    private void Awake() { Time.timeScale = 1f; }

    private void OnEnable()
    {
        EnemyHealth.EnemyKilled += OnEnemyKilled;
        if (player != null) player.Died += OnPlayerDied;
    }

    private void OnDisable()
    {
        EnemyHealth.EnemyKilled -= OnEnemyKilled;
        if (player != null) player.Died -= OnPlayerDied;
    }

    private void Update()
    {
        // Reinicio rapido en cualquier momento (GDD 4.2)
        if (Input.GetKeyDown(KeyCode.R)) Retry();

        if (State != LevelState.Playing) return;

        // Derrota por quedarse sin recursos: sin municion, nada en vuelo y enemigos vivos
        bool noAmmoLeft = weapons != null && !weapons.HasAnyAmmo
                          && Projectile.ActiveCount == 0 && DamageCloud.ActiveCount == 0;
        if (noAmmoLeft && EnemyHealth.Alive.Count > 0) Lose("Sin municion");
    }

    private void OnEnemyKilled(EnemyHealth enemy)
    {
        if (State != LevelState.Playing) return;

        if (EnemyHealth.Alive.Count == 0)
        {
            if (elevator != null)
                elevator.SetActive(true);
        }
    }

    private void OnPlayerDied()
    {
        if (State != LevelState.Playing) return;

        LoseReason = "Eliminado";
        SetState(LevelState.Lost);

        if (defeatText != null)
            defeatText.SetActive(true);

        StartCoroutine(RetryAfter(retryDelay));
    }

    private void Lose(string reason)
    {
        if (State != LevelState.Playing) return;
        LoseReason = reason;
        SetState(LevelState.Lost);
        if (autoRetryDelay >= 0f) StartCoroutine(RetryAfter(autoRetryDelay));
    }

    private void SetState(LevelState newState)
    {
        State = newState;
        Time.timeScale = 0f;
        StateChanged?.Invoke(newState);
    }

    private IEnumerator RetryAfter(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        Retry();
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadNext()
    {
        if (!HasNextLevel) return;
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName);
    }
}
