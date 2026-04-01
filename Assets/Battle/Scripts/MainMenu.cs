using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync("Post");
    }

    public void QuitGame()
    {
        Debug.Log("Game is quitting..."); // cuma keliatan di editor
        Application.Quit();
    }
}
