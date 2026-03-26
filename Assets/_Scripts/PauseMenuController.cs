
using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    public GameObject menuPanel;

    public void OpenMenu()
    {
        Debug.Log("CLICK MENU");
        menuPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ContinueGame()
    {
        menuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}