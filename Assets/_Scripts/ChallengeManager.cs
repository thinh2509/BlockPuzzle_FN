using UnityEngine;
using TMPro;

public enum ChallengeType
{
    ScoreRush,
    Survival,
    FixedObstacles,
    LimitedMove,
    SuddenDeath
}

public class ChallengeManager : MonoBehaviour
{
    public static ChallengeManager Instance;

    public ChallengeType challengeType;

    [Header("Score Rush")]
    public int targetScore = 500;
    public float scoreRushTime = 60f;
    private float currentTime;

    [Header("Limited Move")]
    public int moveLimit = 30;
    private int currentMove;

    [Header("Survival")]
    private float survivalTimer = 0f;

    [Header("Fixed Obstacles")]
    public GameObject[] fixedBlockPrefabs;
    public GridManager gridManager;

    [Header("UI")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI moveText;
    public TextMeshProUGUI targetText;
    public GameObject resultPanel;

    private bool isGameOver = false;
    public bool IsGameOver => isGameOver;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        /*resultPanel.SetActive(false);
        InitMode();*/
        isGameOver = false;
        Time.timeScale = 1f;

        if (resultPanel != null) resultPanel.SetActive(false);

        InitMode();
        ForceRefreshUI(); // đảm bảo Time/Target không bị 0
    }

    void OnEnable()
    {
        // phòng trường hợp object được enable lại (scene load / đổi mode)
        if (!isGameOver) ForceRefreshUI();
    }
    void Update()
    {
        if (isGameOver) return;

        switch (challengeType)
        {
            case ChallengeType.ScoreRush:
                UpdateScoreRush();
                break;

            case ChallengeType.Survival:
                UpdateSurvival();
                break;
        }
    }

    void InitMode()
    {
        if (timeText != null) timeText.gameObject.SetActive(false);
        if (moveText != null) moveText.gameObject.SetActive(false);
        if (targetText != null) targetText.gameObject.SetActive(false);

        switch (challengeType)
        {
            case ChallengeType.ScoreRush:
                currentTime = scoreRushTime;
                timeText.gameObject.SetActive(true);
                targetText.gameObject.SetActive(true);
             
                break;

            case ChallengeType.Survival:
                survivalTimer = 0;
                if (timeText != null) timeText.gameObject.SetActive(true);
                break;

            case ChallengeType.LimitedMove:
                currentMove = moveLimit;
                if (moveText != null) moveText.gameObject.SetActive(true);
                break;

            case ChallengeType.FixedObstacles:
                SpawnFixedObstacles();
                break;

            case ChallengeType.SuddenDeath:
                // mode này thường không cần UI riêng, tùy bạn
                break;

        }
    }

    void ForceRefreshUI()
    {
        // refresh đúng theo mode hiện tại
        if (challengeType == ChallengeType.ScoreRush)
        {
            UpdateTimeUI();
            UpdateTargetUI();
        }
        else if (challengeType == ChallengeType.Survival)
        {
            timeText.text = "Survival: " + Mathf.FloorToInt(survivalTimer);
        }
        else if (challengeType == ChallengeType.LimitedMove)
        {
            UpdateMoveUI();
        }
    }
    // ================= SCORE RUSH =================

    void UpdateScoreRush()
    {
        currentTime -= Time.deltaTime;
        UpdateTimeUI();

        if (currentTime <= 0)
        {
            currentTime = 0f;
            UpdateTimeUI();
            GameOver();
        }
    }

    public void CheckScoreRushWin(int score)
    {
        if (challengeType != ChallengeType.ScoreRush) return;

        if (score >= targetScore)
        {
            WinGame();
        }
    }

    // ================= SURVIVAL =================

    void UpdateSurvival()
    {
        survivalTimer += Time.deltaTime;
        if (timeText != null)
            timeText.text = "Survival: " + Mathf.FloorToInt(survivalTimer);
    }

    // ================= LIMITED MOVE =================

    public void OnBlockPlaced()
    {
        if (isGameOver) return;

        if (challengeType == ChallengeType.LimitedMove)
        {
            currentMove--;
            UpdateMoveUI();

            if (currentMove <= 0)
            {
                GameOver();
            }
        }
    }

    // ================= SUDDEN DEATH =================

    public void OnInvalidPlacement()
    {
        if (isGameOver) return;

        if (challengeType == ChallengeType.SuddenDeath)
        {
            GameOver();
        }
    }

    // ================= FIXED OBSTACLES =================

    void SpawnFixedObstacles()
    {
        if (gridManager == null) return;

        foreach (var block in fixedBlockPrefabs)
        {
            if (block == null) continue;

            int x = UnityEngine.Random.Range(0, gridManager.width);
            int y = UnityEngine.Random.Range(0, 4);

            gridManager.PlaceInitialBlock(block, x, y);
        }
    }

    // ================= UI =================

    void UpdateTimeUI()
    {
        if (timeText != null)
            timeText.text = "Time: " + Mathf.CeilToInt(currentTime);
    }

    void UpdateMoveUI()
    {
        if (moveText != null)
            moveText.text = "Moves: " + currentMove;
    }

    void UpdateTargetUI()
    {
        if (targetText != null)
            targetText.text = "Target: " + targetScore;
    }

    // ================= GAME STATE =================

    void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        // tắt pause nếu đang mở
        if (GameManager.Instance != null && GameManager.Instance.pauseMenuPanel != null)
            GameManager.Instance.pauseMenuPanel.SetActive(false);

        if (resultPanel != null) resultPanel.SetActive(true);
        Time.timeScale = 0f;

        Debug.Log("Game Over");
    }

    void WinGame()
    {
        if (isGameOver) return;
        isGameOver = true;

        // tắt pause nếu đang mở
        if (GameManager.Instance != null && GameManager.Instance.pauseMenuPanel != null)
            GameManager.Instance.pauseMenuPanel.SetActive(false);

        if (resultPanel != null) resultPanel.SetActive(true);
        Time.timeScale = 0f;

        Debug.Log("You Win!");
    }
}


