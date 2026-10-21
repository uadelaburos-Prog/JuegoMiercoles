using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;

public class PauseLogic : MonoBehaviour
{
    [SerializeField] private GameObject pauseCanvas;

    private bool pause;
    private void Awake()
    {
        Time.timeScale = 1.0f;
        pauseCanvas.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(pause) Resume();
            else
            {
                pause = true;
                PauseTouch();
            }
        }
    }

    public void Continue()
    {
        Resume();
    }

    private void Resume()
    {
        pause = false;
        PauseTouch();
    }

    private void PauseTouch()
    {
        pauseCanvas.SetActive(pause);
        Time.timeScale = pause ? 0f : 1f;
    }

    public void QuitGame()
    {
        Application.Quit();
        EditorApplication.isPlaying = false;
    }
}
