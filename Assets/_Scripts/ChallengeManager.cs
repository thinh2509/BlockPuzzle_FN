using Assets._Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChallengeManager : MonoBehaviour
{
    public static ChallengeManager Instance;

    [Header("Refs")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private PieceSpawner pieceSpawner;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private ResultUIManager resultUIManager;
    [SerializeField] private ChallengeDatabase database;
    [SerializeField] private GameObject obstaclePrefab;

    [Header("Debug")]
    [SerializeField] private ChallengeLevelData debugLevel;

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text movesText;
    [SerializeField] private TMP_Text modeText;
    [SerializeField] private TMP_Text levelText;

    [Header("Fixed Obstacle Shapes")]
    [SerializeField] private GameObject[] fixedObstacleShapePrefabs;
    [SerializeField] private int randomShapeCount = 3;

    public bool IsGameOver { get; private set; }

    private ChallengeLevelData currentLevel;
    private float timeLeft;
    private int movesLeft;

    private const float SuddenDeathMaxTime = 300f;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        if (scoreManager != null)
            scoreManager.OnScoreChanged += HandleScoreChanged;
    }

    private void OnDisable()
    {
        if (scoreManager != null)
            scoreManager.OnScoreChanged -= HandleScoreChanged;
    }

    private void Start()
    {
        if (ChallengeSession.SelectedLevel == null)
        {
            if (debugLevel != null)
            {
                Debug.LogWarning("SelectedLevel null, dùng debugLevel.");
                StartChallenge(debugLevel);
                return;
            }

            Debug.LogError("ChallengeSession.SelectedLevel is null.");
            return;
        }

        StartChallenge(ChallengeSession.SelectedLevel);
    }

    private void Update()
    {
        if (IsGameOver || currentLevel == null)
            return;

        UpdateTimer();
        CheckWinCondition();

        RefreshUI();
    }

    public void StartChallenge(ChallengeLevelData level)
    {
        currentLevel = level;
        IsGameOver = false;
        Time.timeScale = 1f;

        if (resultUIManager != null)
            resultUIManager.HideAll();

        if (gridManager != null)
            gridManager.ClearAllBlocks();

        if (scoreManager != null)
            scoreManager.ResetScore();

        timeLeft = currentLevel.mode == ChallengeModeType.SuddenDeath
            ? SuddenDeathMaxTime
            : currentLevel.timeLimit;

        movesLeft = currentLevel.moveLimit;

        SetupMode();

        if (pieceSpawner != null)
            pieceSpawner.ResetSpawner();

        // Ẩn 2 dòng gây khó chịu
        if (modeText != null) modeText.text = "";
        if (levelText != null) levelText.text = "";

        RefreshUI();
    }

    private void SetupMode()
    {
        if (currentLevel.mode != ChallengeModeType.FixedObstacles || gridManager == null)
            return;

        // obstacle 1x1 cũ nếu vẫn muốn giữ
        if (currentLevel.obstacleCount > 0 && obstaclePrefab != null)
        {
            gridManager.PlaceObstacleRandom(currentLevel.obstacleCount, obstaclePrefab);
        }

        // obstacle shape nhiều ô
        if (fixedObstacleShapePrefabs != null &&
            fixedObstacleShapePrefabs.Length > 0 &&
            randomShapeCount > 0)
        {
            gridManager.PlaceRandomObstacleShapes(randomShapeCount, fixedObstacleShapePrefabs);
        }
    }




    private void UpdateTimer()
    {
        switch (currentLevel.mode)
        {
            case ChallengeModeType.ScoreRush:
            case ChallengeModeType.Survival:
            case ChallengeModeType.FixedObstacles:
            case ChallengeModeType.SuddenDeath:
                timeLeft -= Time.deltaTime;
                if (timeLeft < 0) timeLeft = 0;

                if (timeLeft <= 0)
                    HandleTimeUp();
                break;
        }
    }

    private void CheckWinCondition()
    {
        if (IsGameOver || currentLevel == null || scoreManager == null)
            return;

        switch (currentLevel.mode)
        {
            case ChallengeModeType.ScoreRush:
            case ChallengeModeType.FixedObstacles:
            case ChallengeModeType.LimitedMoves:
                if (scoreManager.CurrentScore >= currentLevel.targetScore && movesLeft > 0)
                    WinLevel();
                break;

            case ChallengeModeType.Survival:
                if (scoreManager.CurrentScore >= currentLevel.targetScore && timeLeft <= 0)
                    WinLevel();
                break;

            case ChallengeModeType.SuddenDeath:
                break;
        }
    }

    private void HandleScoreChanged(int score)
    {
        if (IsGameOver || currentLevel == null)
            return;

        if (currentLevel.mode == ChallengeModeType.LimitedMoves)
        {
            if (score % 100 == 0)  
            {
                movesLeft++;
                ChallengeManager.Instance.movesLeft = movesLeft;
                Debug.Log("Combo achieved! Move added.");
            }

            if (score >= currentLevel.targetPointsForLimitedMoves && movesLeft <= currentLevel.maxMovesForLimitedMoves)
            {
                movesLeft++;
                Debug.Log("500 points in 10 moves condition achieved!");
            }
        }
    


        switch (currentLevel.mode)
        {
            case ChallengeModeType.ScoreRush:
            case ChallengeModeType.FixedObstacles:
            case ChallengeModeType.LimitedMoves:
                if (score >= currentLevel.targetScore)
                    WinLevel();
                break;

            case ChallengeModeType.Survival:
                break;

            case ChallengeModeType.SuddenDeath:
                break;
        }

        RefreshUI();
    }

    private void HandleTimeUp()
    {
        if (IsGameOver || currentLevel == null || scoreManager == null)
            return;

        switch (currentLevel.mode)
        {
            case ChallengeModeType.ScoreRush:
            case ChallengeModeType.FixedObstacles:
                if (scoreManager.CurrentScore >= currentLevel.targetScore)
                    WinLevel();
                else
                    LoseLevel();
                break;

            case ChallengeModeType.Survival:
                if (scoreManager.CurrentScore >= currentLevel.targetScore)
                    WinLevel();
                else
                    LoseLevel();
                break;

            case ChallengeModeType.SuddenDeath:
                LoseLevel();
                break;

            case ChallengeModeType.LimitedMoves:
                if (scoreManager.CurrentScore >= currentLevel.targetScore && movesLeft > 0)
                    WinLevel();
                else
                    LoseLevel();
                break;
        }
    }

    public void UseMove()
    {
        if (IsGameOver || currentLevel == null || scoreManager == null)
            return;

        if (currentLevel.mode != ChallengeModeType.LimitedMoves)
            return;

        movesLeft--;

        if (scoreManager.CurrentScore % 100 == 0)
        {
            movesLeft++;  // Cộng thêm 1 lượt move khi đạt combo
            Debug.Log("Combo achieved! Move added.");
        }
        RefreshUI();

        if (scoreManager.CurrentScore >= currentLevel.targetScore && movesLeft > 0)
        {
            WinLevel();
            return;
        }

        if (movesLeft <= 0)
            LoseLevel();
    }

    public void HandleNoMovesLeft()
    {
        if (IsGameOver || currentLevel == null || scoreManager == null)
            return;

        switch (currentLevel.mode)
        {
            case ChallengeModeType.ScoreRush:
            case ChallengeModeType.FixedObstacles:
            case ChallengeModeType.LimitedMoves:
                if (scoreManager.CurrentScore >= currentLevel.targetScore)
                    WinLevel();
                else
                    LoseLevel();
                break;

            case ChallengeModeType.Survival:
                LoseLevel();
                break;

            case ChallengeModeType.SuddenDeath:
                LoseLevel();
                break;
        }
    }

    private void WinLevel()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        Time.timeScale = 0f;
        Debug.Log("CHALLENGE WIN");

        if (database != null && currentLevel != null)
            ChallengeProgressService.CompleteLevel(currentLevel, database);

        if (resultUIManager != null)
            resultUIManager.ShowWin();
        else
            Debug.LogError("resultUIManager is NULL");
    }

    private void LoseLevel()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        Time.timeScale = 0f;
        Debug.Log("CHALLENGE LOSE");

        if (resultUIManager != null)
            resultUIManager.ShowLose();
        else
            Debug.LogError("resultUIManager is NULL");
    }

    public void HandleLose()
    {
        if (IsGameOver) return;
        LoseLevel();
    }


    private void RefreshUI()
    {
        if (scoreText != null && scoreManager != null)
        {
            scoreText.text =
                "<color=#55FF55>S</color>" +
                "<color=#55AAFF>C</color>" +
                "<color=#FFAA00>O</color>" +
                "<color=#FFDD55>R</color>" +
                "<color=#FF5555>E</color>: " +
                $"<color=white>{scoreManager.CurrentScore}</color>";
        }

        if (targetText != null && currentLevel != null)
        {
            if (currentLevel.mode == ChallengeModeType.SuddenDeath)
            {
                targetText.text =
                    "<color=#FFDD55>T</color>" +
                    "<color=#FFAA00>A</color>" +
                    "<color=#55AAFF>R</color>" +
                    "<color=#55FF55>G</color>" +
                    "<color=#FF5555>E</color>" +
                    "<color=#FFD700>T</color>: " +
                    "<color=white>--</color>";
            }
            else
            {
                targetText.text =
                    "<color=#FFDD55>T</color>" +
                    "<color=#FFAA00>A</color>" +
                    "<color=#55AAFF>R</color>" +
                    "<color=#55FF55>G</color>" +
                    "<color=#FF5555>E</color>" +
                    "<color=#FFD700>T</color>: " +
                    $"<color=white>{currentLevel.targetScore}</color>";
            }
        }

        if (timeText != null && currentLevel != null)
        {
            if (currentLevel.mode == ChallengeModeType.LimitedMoves)
            {
                timeText.text = "";
            }
            else
            {
                timeText.text =
                    "<color=#FF5555>T</color>" +
                    "<color=#55AAFF>I</color>" +
                    "<color=#FFAA00>M</color>" +
                    "<color=#FFDD55>E</color>: " +
                    $"<color=white>{Mathf.CeilToInt(timeLeft)}</color>";
            }
        }

        if (movesText != null && currentLevel != null)
        {
            if (currentLevel.mode == ChallengeModeType.LimitedMoves)
            {
                movesText.text =
                    "<color=#FFAA00>M</color>" +
                    "<color=#55AAFF>O</color>" +
                    "<color=#FFDD55>V</color>" +
                    "<color=#55FF55>E</color>" +
                    "<color=#FF5555>S</color>: " +
                    $"<color=white>{movesLeft}</color>";
            }
            else
            {
                movesText.text = "";
            }
        }

        if (levelText != null && currentLevel != null)
        {
            levelText.text =
                "<color=#FFD700>L</color>" +
                "<color=#FFAA00>E</color>" +
                "<color=#55AAFF>V</color>" +
                "<color=#55FF55>E</color>" +
                "<color=#FF5555>L</color>: " +
                $"<color=white>{currentLevel.displayName}</color>";
        }

        if (modeText != null)
        {
            modeText.text = "";
        }

        /*if (currentLevel.mode == ChallengeModeType.LimitedMoves)
        {
            if (modeText != null)
            {
                modeText.text =
                    "<color=#FFD700>Condition:</color>" +
                    $" <color=white>Reach {currentLevel.targetPointsForLimitedMoves} points in {currentLevel.maxMovesForLimitedMoves} moves to get bonus points!</color>";
            }

            
            if (scoreManager.CurrentScore >= currentLevel.targetPointsForLimitedMoves && movesLeft <= currentLevel.maxMovesForLimitedMoves)
            {
                if (modeText != null)
                {
                    modeText.text += "\n<color=#00FF00>Bonus: Points achieved, Move added!</color>";
                }
            }

            if (modeText != null)
            {
                RectTransform modeRectTransform = modeText.GetComponent<RectTransform>();
                if (modeRectTransform != null)
                {
                    modeRectTransform.anchorMin = new Vector2(1, 1);  // Anchor to top-right
                    modeRectTransform.anchorMax = new Vector2(1, 1);  // Anchor to top-right
                    modeRectTransform.anchoredPosition = new Vector2(-10, -10);  // Adjust position slightly for spacing
                }
            }

        }*/

        if (currentLevel.mode == ChallengeModeType.LimitedMoves)
        {
            // Hiển thị điều kiện phụ: Đạt 500 điểm trong 10 bước
            if (modeText != null)
            {
                modeText.text =
                    "<color=#FFD700>Condition:</color>" +
                    $" <color=white>Reach {currentLevel.targetPointsForLimitedMoves} points in {currentLevel.maxMovesForLimitedMoves} moves to get bonus points!</color>";

                // Set font size to smaller
                modeText.fontSize = 20; // Adjust as needed to make it smaller
            }

            // Hiển thị thông báo nếu người chơi đã hoàn thành điều kiện phụ (ví dụ: Combo)
            if (scoreManager.CurrentScore >= currentLevel.targetPointsForLimitedMoves && movesLeft <= currentLevel.maxMovesForLimitedMoves)
            {
                if (modeText != null)
                {
                    modeText.text += "\n<color=#00FF00>Bonus: Points achieved, Move added!</color>";
                }
            }

            // Move the condition to the top-right corner near the grid
            if (modeText != null)
            {
                RectTransform modeRectTransform = modeText.GetComponent<RectTransform>();
                if (modeRectTransform != null)
                {
                    modeRectTransform.anchorMin = new Vector2(1, 1);  // Căn vào góc phải
                    modeRectTransform.anchorMax = new Vector2(1, 1);  // Căn vào góc phải
                    modeRectTransform.pivot = new Vector2(1, 1); // Căn điểm pivot tại góc phải trên
                    modeRectTransform.anchoredPosition = new Vector2(-20, -100);
                }
            }
        }
    }

    

    public ChallengeLevelData GetCurrentLevel()
    {
        return currentLevel;
    }

    public void RetryLevel()
    {
        if (currentLevel == null) return;
        StartChallenge(currentLevel);
    }

    public void StartNextLevel()
    {
        if (database == null || currentLevel == null) return;

        var next = database.GetNextLevel(currentLevel);
        if (next == null)
        {
            Debug.Log("All challenge levels complete.");
            SceneManager.LoadScene("MainMenu");
            return;
        }

        ChallengeSession.SelectedLevel = next;
        SceneManager.LoadScene("Challenge");
    }
}