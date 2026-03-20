using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System; // For Exception

public class GameManager1v1 : MonoBehaviour
{
    public static GameManager1v1 Instance;

    [Header("UI Elements")]
    public TextMeshProUGUI myScoreText;
    public TextMeshProUGUI opponentScoreText;

    private string roomId;
    private string myUserId;
    private string player1Id; // From room data
    private string player2Id; // From room data

    private const string ApiBaseUrl = "https://localhost:7051/api/Room";

    [System.Serializable]
    public class RoomResponse
    {
        public string id;
        public string roomCode;
        public string player1Id;
        public string player2Id;
        public int player1Score;
        public int player2Score;
        public string status;
        public DateTime createdAt;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Only if this script persists across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Retrieve roomId and myUserId from PlayerPrefs
        roomId = PlayerPrefs.GetString("roomId");
        myUserId = PlayerPrefs.GetString("userId");

        if (string.IsNullOrEmpty(roomId) || string.IsNullOrEmpty(myUserId))
        {
            Debug.LogError("RoomId or UserId not found in PlayerPrefs. Cannot start 1v1 game.");
            // Handle error, e.g., return to main menu
            return;
        }

        StartCoroutine(FetchOpponentScorePeriodically());
    }

    public void UpdateMyScore(int scoreToAdd)
    {
        StartCoroutine(AddScoreCoroutine(scoreToAdd));
    }

    private IEnumerator AddScoreCoroutine(int scoreToAdd)
    {
        // Prepare the request body
        string jsonBody = JsonUtility.ToJson(new { roomId = this.roomId, userId = this.myUserId, score = scoreToAdd });
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest($"{ApiBaseUrl}/add-score", "PUT"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // Add authorization token if needed (assuming AuthManager.Instance exists and is logged in)
            if (AuthManager.Instance != null && AuthManager.Instance.IsLoggedIn)
            {
                request.SetRequestHeader("Authorization", "Bearer " + AuthManager.Instance.AuthToken);
            }
            request.certificateHandler = new BypassCertificateHandler(); // Assuming this is needed

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error adding score: {request.error}. Response: {request.downloadHandler.text}");
            }
            else
            {
                Debug.Log($"Score {scoreToAdd} added successfully for user {myUserId} in room {roomId}.");
            }
        }
    }

    private IEnumerator FetchOpponentScorePeriodically()
    {
        while (true)
        {
            using (UnityWebRequest request = UnityWebRequest.Get($"{ApiBaseUrl}/{roomId}"))
            {
                request.certificateHandler = new BypassCertificateHandler(); // Assuming this is needed
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    RoomResponse roomData = JsonUtility.FromJson<RoomResponse>(request.downloadHandler.text);
                    if (roomData != null)
                    {
                        player1Id = roomData.player1Id;
                        player2Id = roomData.player2Id;

                        if (myUserId == player1Id)
                        {
                            myScoreText.text = $"My Score: {roomData.player1Score}";
                            opponentScoreText.text = $"Opponent Score: {roomData.player2Score}";
                        }
                        else if (myUserId == player2Id)
                        {
                            myScoreText.text = $"My Score: {roomData.player2Score}";
                            opponentScoreText.text = $"Opponent Score: {roomData.player1Score}";
                        }
                        else
                        {
                            Debug.LogWarning("Current user is neither Player1 nor Player2 in this room.");
                        }
                    }
                }
                else
                {
                    Debug.LogError($"Error fetching room data: {request.error}");
                }
            }
            yield return new WaitForSeconds(1f); // Fetch every 1 second
        }
    }
}
