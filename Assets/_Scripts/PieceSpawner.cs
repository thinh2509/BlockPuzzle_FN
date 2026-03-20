using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Text;

public class PieceSpawner : MonoBehaviour
{
    [Header("Cài đặt Gạch")]
    public GameObject[] blockPrefabs;
    public Transform[] spawnPoints;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;

    private readonly List<GameObject> spawnedPieces = new List<GameObject>();
    private GridManager gridManager;
    private bool isGameOver = false;

    private const string ApiBaseUrl = "https://localhost:7051/api/Score";

    [System.Serializable]
    private class ScoreData { public int score; }

    

    void Start()
    {
        gridManager = FindFirstObjectByType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError("GridManager not found!");
            return;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        SpawnBlocks();
    }

    public void SpawnBlocks()
    {
        if (isGameOver) return;
        foreach (var piece in spawnedPieces)
        {
            if (piece != null)
                Destroy(piece);
        }
        spawnedPieces.Clear();

        foreach (Transform point in spawnPoints)
        {
            int randomIndex = Random.Range(0, blockPrefabs.Length);
            GameObject randomBlockPrefab = blockPrefabs[randomIndex];

            GameObject newPiece = Instantiate(randomBlockPrefab, point.position, Quaternion.identity, transform);
            newPiece.transform.localScale = Vector3.one;
            spawnedPieces.Add(newPiece);
        }

        CheckForGameOver();
    }

    public void OnPiecePlaced(GameObject piece)
    {
        if (spawnedPieces.Contains(piece))
        {
            spawnedPieces.Remove(piece);
        }

        

        if (spawnedPieces.Count == 0 &&  !isGameOver)
        {
            SpawnBlocks();
            return;
        }
        
            CheckForGameOver();
    }

    public GameObject[] GetCurrentPieces()
    {
        List<GameObject> validPieces = new List<GameObject>();

        foreach (var piece in spawnedPieces)
        {
            if (piece != null)
                validPieces.Add(piece);
        }

        return validPieces.ToArray();
    }

    void CheckForGameOver()
    {
        if (isGameOver) return;
        if (gridManager == null) return;
        if (spawnedPieces.Count == 0) return;

        bool hasValidMove = gridManager.CanPlaceAnyBlock(GetCurrentPieces());

        if (!hasValidMove)
        {
            isGameOver = true;

            if (GameManager.Instance != null &&
                GameManager.Instance.currentMode == GameManager.GameMode.Challenge)
            {
                if (ChallengeManager.Instance != null)
                  ChallengeManager.Instance.HandleNoMovesLeft();
            }
            else
            {
                StartCoroutine(HandleGameOver());
            }
        }
    }

    public void ResetSpawner()
    {
        isGameOver = false;
        SpawnBlocks();
    }

    private IEnumerator HandleGameOver()
    {
        // First, submit the score
        if (AuthManager.Instance != null && AuthManager.Instance.IsLoggedIn)
        {
            yield return StartCoroutine(SubmitScoreCoroutine());
        }

        // Then, show the game over panel
        if (gameOverPanel != null)
        {
            if (finalScoreText != null && ScoreManager.Instance != null)
            {
                finalScoreText.text = "Score: " + ScoreManager.Instance.CurrentScore;
            }
            gameOverPanel.SetActive(true);
        }
        Time.timeScale = 0f;
    }

    private IEnumerator SubmitScoreCoroutine()
    {
        if (ScoreManager.Instance == null) yield break;
        int currentScore = ScoreManager.Instance.CurrentScore;
        if (currentScore <= 0) yield break; // Don't submit zero or negative scores

        ScoreData data = new ScoreData { score = currentScore };
        string jsonBody = JsonUtility.ToJson(data);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest($"{ApiBaseUrl}/add", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            // Add the authorization token
            string token = AuthManager.Instance.AuthToken;
            request.SetRequestHeader("Authorization", "Bearer " + token);
            
            request.certificateHandler = new BypassCertificateHandler();

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error submitting score: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
            }
            else
            {
                Debug.Log("Score submitted successfully!");
            }
        }
    }
}