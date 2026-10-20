using UnityEngine;


public class Elevator : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (levelManager == null) return;

        levelManager.LoadNext();
    }
}
