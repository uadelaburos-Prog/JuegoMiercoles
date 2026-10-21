using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class StartMenuLogic : MonoBehaviour
{
    public void NextScene()
    {
        SceneManager.LoadScene("Scene1");
    }

    public void QuitGame()
    {
        Application.Quit();
        EditorApplication.isPlaying = false;
    }
}
