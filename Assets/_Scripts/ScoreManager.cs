using UnityEngine;
using TMPro;
using System;
using System.Collections;
using System.Text;
using UnityEngine.Networking;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int CurrentScore { get; private set; } = 0;
    public int ComboCount { get; private set; } = 0;

    public event Action<int> OnScoreChanged;

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI comboText;

    private const string ApiBaseUrl = "https://localhost:7051/api/Score";

    [System.Serializable]
    private class ScoreData { public int score; }

    private void Awake()
    {
        // Singleton đơn giản cho từng màn chơi
        // Nếu có 1 cái khác đang tồn tại thì tự hủy cái này đi để tránh trùng lặp
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            // QUAN TRỌNG: Đã XÓA dòng DontDestroyOnLoad để nó tự reset khi Replay
        }
    }

    private void Start()
    {
        ResetScore();
    }

    public void ResetScore()
    {
        CurrentScore = 0;
        ComboCount = 0;
        UpdateScoreUI();
        UpdateComboUI();
        OnScoreChanged?.Invoke(CurrentScore);
    }
    public void AddPoints(int points)
    {
        if (ChallengeManager.Instance != null && ChallengeManager.Instance.IsGameOver)
            return;
        CurrentScore += points;
        UpdateScoreUI();
        OnScoreChanged?.Invoke(CurrentScore);

        Debug.Log($"[Tracer] ScoreManager about to send to MultiplayerManager! Instance is {(MultiplayerManager.Instance == null ? "NULL" : "VALID")}");

        if (MultiplayerManager.Instance != null)
        {
            MultiplayerManager.Instance.SendScoreUpdate(CurrentScore);
        }

        /*if (ChallengeManager.Instance != null)
        {
            ChallengeManager.Instance.CheckScoreRushWin(CurrentScore);
        }*/
    }

    public void IncrementCombo()
    {
        ComboCount++;
        UpdateComboUI();
    }

    public void ResetCombo()
    {
        ComboCount = 0;
        UpdateComboUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "<color=#55AAFF>S</color><color=#FF5555>C</color><color=#FFAA00>O</color><color=#55FF55>R</color><color=#FFDD55>E</color>: " + CurrentScore.ToString();
        }
    }

    private void UpdateComboUI()
    {
        if (comboText != null)
        {
            if (ComboCount > 0)
            {
                comboText.text = "<color=#FF5555>C</color><color=#55AAFF>o</color><color=#FFAA00>m</color><color=#FFDD55>b</color><color=#55FF55>o</color>: x<color=white>" + (ComboCount + 1).ToString() + "</color>";
            }
            else
            {
                comboText.text = ""; // Xóa text khi không có combo
            }
        }
    }

    public void SubmitScore()
    {
        if (AuthManager.Instance != null && AuthManager.Instance.IsLoggedIn)
        {
            StartCoroutine(SubmitScoreCoroutine());
        }
        else
        {
            Debug.LogWarning("ScoreManager: User not logged in, score will not be saved.");
        }
    }

    private IEnumerator SubmitScoreCoroutine()
    {
        if (CurrentScore <= 0) yield break;

        ScoreData data = new ScoreData { score = CurrentScore };
        string jsonBody = JsonUtility.ToJson(data);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest($"{ApiBaseUrl}/add", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (AuthManager.Instance != null && AuthManager.Instance.IsLoggedIn)
            {
                string token = AuthManager.Instance.AuthToken;
                request.SetRequestHeader("Authorization", "Bearer " + token);
            }

            request.certificateHandler = new BypassCertificateHandler();

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("ScoreManager: Error submitting score: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
            }
            else
            {
                Debug.Log("ScoreManager: Score submitted successfully!");
            }
        }
    }
}