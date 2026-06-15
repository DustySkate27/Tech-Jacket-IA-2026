
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public void GoToMenu()
    {
        SceneManager.LoadScene("menu");
    }

    public void LoadFirstLevel()
    {
        SceneManager.LoadScene("menu");
    }

    public void LoadSecondtLevel()
    {
        SceneManager.LoadScene("menu");
    }

    public void ResetCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(sceneName);

    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
