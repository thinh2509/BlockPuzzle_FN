using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Text;
using UnityEngine.Networking;



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

    private const string ApiBaseUrl = "https://localhost:7051/api/Score";

    // Helper classes for JSON deserialization
    [System.Serializable]
    private class ScoreRecord
    {
        public int score;
        public string date; // Receive as string
    }

    [System.Serializable]
    private class ScoreList
    {
        public ScoreRecord[] items;
    }


    void Start()
    {
        // Add listeners to buttons, with null checks for safety
        if (playButton != null) playButton.onClick.AddListener(PlayGame);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (highScoresButton != null) highScoresButton.onClick.AddListener(OnHighScoresButtonClick);
        if (closeHighScoresButton != null) closeHighScoresButton.onClick.AddListener(CloseHighScoresPanel);
        if (quitButton != null) quitButton.onClick.AddListener(Logout);

        // Initially hide the panel
        if (highScoresPanel != null)
        {
            highScoresPanel.SetActive(false);
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Gameplay");
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

    public void OpenSettings()
    {
        // TODO: Implement settings functionality
        Debug.Log("Settings button clicked.");
    }

    private void OnHighScoresButtonClick()
    {
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

    private void CloseHighScoresPanel()
    {
        if (highScoresPanel != null)
        {
            highScoresPanel.SetActive(false);
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
