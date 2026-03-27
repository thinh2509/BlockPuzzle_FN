using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;




public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject pauseMenuPanel;
    public GameObject gameplayGameOverUI;
    public GameObject challengeResultUI;
    public TMP_Text challengeResultText;
 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public enum GameMode
    {
        Gameplay,
        Challenge
    }

    public GameMode currentMode;

    void Start()
    {
        // QUAN TRỌNG: đảm bảo các panel kết quả được ẩn từ đầu game
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (gameplayGameOverUI != null) gameplayGameOverUI.SetActive(false);
        if (challengeResultUI != null) challengeResultUI.SetActive(false);
        
        Time.timeScale = 1f;
    }
    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        // Tự động lưu điểm nếu cần trước khi reset (tùy chọn)
        // ScoreManager.Instance.SubmitScore(); 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    public void OpenPauseMenu()
    {
        if (ChallengeManager.Instance != null && ChallengeManager.Instance.IsGameOver) return;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }


    public void ResumeGame()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false); 
            Time.timeScale = 1f;
        }
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SubmitScore();
        }
        SceneManager.LoadScene("MainMenu");
    }

    public void HandleGameOver()
    {
        if (currentMode == GameMode.Gameplay)
        {
            if (gameplayGameOverUI != null)
                gameplayGameOverUI.SetActive(true);
        }
        else if (currentMode == GameMode.Challenge)
        {
            if (ChallengeManager.Instance != null)
                ChallengeManager.Instance.HandleLose();
        }

        Time.timeScale = 0f;
    }

    public void ShowChallengeResult(bool win)
    {
        if (challengeResultUI != null)
            challengeResultUI.SetActive(true);

        if (challengeResultText != null)
            challengeResultText.text = win ? "YOU WIN" : "GAME OVER";

        Time.timeScale = 0f;
    }

}