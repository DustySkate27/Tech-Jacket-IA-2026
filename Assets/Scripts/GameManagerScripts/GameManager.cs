
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        EventBus.Subscribe<OnPlayerDeath>(ResetCurrentScene);
        EventBus.Subscribe<OnPlayerWin>(GoToMenu);
    }

    public void GoToMenu(OnPlayerWin onPlayerWin)
    {
        SceneManager.LoadScene("menu");
    }

    public void LoadFirstLevel()
    {
        SceneManager.LoadScene("Theta");
    }

    public void LoadSecondtLevel()
    {
        SceneManager.LoadScene("menu");
    }

    public void ResetCurrentScene(OnPlayerDeath onPlayerDeath)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(sceneName);

    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
