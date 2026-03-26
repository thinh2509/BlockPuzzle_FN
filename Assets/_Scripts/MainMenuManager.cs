    using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Text;
using UnityEngine.Networking;
using Assets._Scripts;



public class MainMenuManager : MonoBehaviour
{
    [Header("Main Buttons")]
    public Button playButton;
    public Button settingsButton;
    public Button highScoresButton;
    public Button quitButton;

    [Header("High Scores UI")]
    public GameObject highScoresPanel;
    public TextMeshProUGUI highScoresText;
    public Button closeHighScoresButton;

    [Header("Challenge Test")]
    [SerializeField] private ChallengeLevelData testLevel;
    private const string ApiBaseUrl = "https://localhost:7051/api/Score";

    [System.Serializable]
    private class ScoreRecord
    {
        public int score;
        public string date; 
    }

    [System.Serializable]
    private class ScoreList
    {
        public ScoreRecord[] items;
    }

    [SerializeField] private GameObject settingsPopup;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private CanvasGroup mainMenuCanvasGroup;

    [Header("Sound Settings")]
    public AudioClip buttonClickSound;
    [SerializeField] private Button closeSettingsButton;
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle soundToggle;
    [SerializeField] private Toggle Toggle_Vibration;

    public void OpenSettings()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
        if (highScoresPanel != null && highScoresPanel.activeSelf)
        {
            highScoresPanel.SetActive(false);
        }
        settingsPopup.SetActive(true);
        
        // Disable interaction and dim the main menu via CanvasGroup
        if (mainMenuCanvasGroup != null)
        {
            mainMenuCanvasGroup.alpha = 0.5f; // Dim the main menu
            mainMenuCanvasGroup.interactable = false; // Disable interaction
            mainMenuCanvasGroup.blocksRaycasts = false; // Prevent raycasts from hitting main menu
        }

        // Initialize toggle states when settings open
        if (musicToggle != null) musicToggle.isOn = SettingsManager.Instance.IsMusicOn;
        if (soundToggle != null) soundToggle.isOn = SettingsManager.Instance.IsSoundOn;

    }

    public void HideSettingsPopup()
    {
        if (settingsPopup != null)
        {
            settingsPopup.SetActive(false); // Deactivate settings popup
        }
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true); // Re-activate main menu when settings close
        }

        // Re-enable interaction and restore full visibility of the main menu
        if (mainMenuCanvasGroup != null)
        {
            mainMenuCanvasGroup.alpha = 1f; // Restore full visibility
            mainMenuCanvasGroup.interactable = true; // Re-enable interaction
            mainMenuCanvasGroup.blocksRaycasts = true; // Allow raycasts to hit main menu
        }
        Debug.Log("MainMenuManager: Hiding Settings Popup.");
    }

    void Start()
    {
       // PlayerPrefs.SetString("userId", "1");
        // Add listeners to buttons, with null checks for safety
        if (playButton != null) playButton.onClick.AddListener(PlayGame);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (highScoresButton != null) highScoresButton.onClick.AddListener(OnHighScoresButtonClick);
        if (closeHighScoresButton != null) closeHighScoresButton.onClick.AddListener(CloseHighScoresPanel);
        if (quitButton != null) quitButton.onClick.AddListener(Logout);
        
        // Add click sound listeners for main menu buttons
        if (playButton != null && buttonClickSound != null) playButton.onClick.AddListener(() => AudioManager.Instance.PlaySFX(buttonClickSound));
        if (settingsButton != null && buttonClickSound != null) settingsButton.onClick.AddListener(() => AudioManager.Instance.PlaySFX(buttonClickSound));
        if (highScoresButton != null && buttonClickSound != null) highScoresButton.onClick.AddListener(() => AudioManager.Instance.PlaySFX(buttonClickSound));
        if (quitButton != null && buttonClickSound != null) quitButton.onClick.AddListener(() => AudioManager.Instance.PlaySFX(buttonClickSound));


        // Add click sound listeners for settings panel buttons/toggles
        if (closeSettingsButton != null && buttonClickSound != null) closeSettingsButton.onClick.AddListener(() => AudioManager.Instance.PlaySFX(buttonClickSound));
        if (musicToggle != null && buttonClickSound != null) musicToggle.onValueChanged.AddListener((isOn) => AudioManager.Instance.PlaySFX(buttonClickSound));
        if (soundToggle != null && buttonClickSound != null) soundToggle.onValueChanged.AddListener((isOn) => AudioManager.Instance.PlaySFX(buttonClickSound));
        if (Toggle_Vibration != null && buttonClickSound != null) Toggle_Vibration.onValueChanged.AddListener((isOn) => AudioManager.Instance.PlaySFX(buttonClickSound));


        {
            if (!PlayerPrefs.HasKey("userId"))
            {
                PlayerPrefs.SetString("userId", Random.Range(1, 9999).ToString());
                PlayerPrefs.Save();
            }

            Debug.Log("UserId: " + PlayerPrefs.GetString("userId"));
        }

        // Initially hide the panel
        if (highScoresPanel != null)
        {
            highScoresPanel.SetActive(false);
        }

        // Ensure main menu is fully visible and interactable at start
        if (mainMenuCanvasGroup != null)
        {
            mainMenuCanvasGroup.alpha = 1f;
            mainMenuCanvasGroup.interactable = true;
            mainMenuCanvasGroup.blocksRaycasts = true;
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Gameplay");
    }

    public void Challenge()
    {
        if (testLevel == null)
        {
            Debug.LogError("Test Level  Inspector.");
            return;
        }

        ChallengeSession.SelectedLevel = testLevel;
        SceneManager.LoadScene("Challenge");
    }

    public void Logout()
    {
        // Clear the token from the AuthManager
        if (AuthManager.Instance != null)
        {
            AuthManager.Instance.ClearToken();
        }

        // Load the Login scene
        SceneManager.LoadScene("Login");
    }


    private void OnHighScoresButtonClick()
    {
        // Deactivate settingsPopup if it's active
        if (settingsPopup != null && settingsPopup.activeSelf)
        {
            settingsPopup.SetActive(false);
        }

        if (AuthManager.Instance == null || !AuthManager.Instance.IsLoggedIn)
        {
            // Optionally, redirect to login or show a message
            Debug.LogWarning("User not logged in. Cannot fetch high scores.");
            // For now, just show the panel with a message
            if (highScoresPanel != null)
            {
                highScoresPanel.SetActive(true);
                if(highScoresText != null) highScoresText.text = "Please log in to see high scores.";
            }
            return;
        }

        StartCoroutine(FetchHighScoresCoroutine());
    }

    public void CloseHighScoresPanel()
    {
        if (highScoresPanel != null)
        {
            highScoresPanel.SetActive(false);
        }
        // Re-activate main menu when high scores close
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
        // Re-enable interaction and restore full visibility of the main menu
        if (mainMenuCanvasGroup != null)
        {
            mainMenuCanvasGroup.alpha = 1f; // Restore full visibility
            mainMenuCanvasGroup.interactable = true; // Re-enable interaction
            mainMenuCanvasGroup.blocksRaycasts = true; // Allow raycasts to hit main menu
        }
    }

    private IEnumerator FetchHighScoresCoroutine()
    {
        if (highScoresPanel != null)
        {
            highScoresPanel.SetActive(true);
            if(highScoresText != null) highScoresText.text = "Loading...";
        }

        using (UnityWebRequest request = new UnityWebRequest($"{ApiBaseUrl}/highscores", "GET"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // Add the authorization token
            string token = AuthManager.Instance.AuthToken;
            request.SetRequestHeader("Authorization", "Bearer " + token);

            request.certificateHandler = new BypassCertificateHandler();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                
                // JsonUtility cannot deserialize a root array directly, so we wrap it.
                // The API returns an array like [{}, {}], so we manually wrap it in `{"items":...}`
                string wrappedJson = "{\"items\":" + jsonResponse + "}";
                ScoreList scoreList = JsonUtility.FromJson<ScoreList>(wrappedJson);

                if (scoreList != null && scoreList.items != null && scoreList.items.Length > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("<b><color=#FFAA00>High Scores</color></b>");
                    sb.AppendLine("-----------------");
                    int rank = 1;
                    foreach (var record in scoreList.items)
                    {
                        try
                        {
                            // Basic date formatting
                            string formattedDate = System.DateTime.Parse(record.date).ToString("yyyy-MM-dd");
                            sb.AppendLine($"#{rank}: {record.score}  <size=20>({formattedDate})</size>");
                        }
                        catch (System.Exception)
                        {
                            // Fallback if date parsing fails
                            sb.AppendLine($"#{rank}: {record.score}");
                        }
                        rank++;
                    }
                    if(highScoresText != null) highScoresText.text = sb.ToString();
                }
                else
                {
                    if(highScoresText != null) highScoresText.text = "No high scores recorded yet.";
                }
            }
            else
            {
                if(highScoresText != null) highScoresText.text = "Error fetching scores: " + request.error;
            }
        }
    }


}
