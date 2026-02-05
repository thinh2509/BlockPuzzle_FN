using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int CurrentScore { get; private set; } = 0;
    public int ComboCount { get; private set; } = 0;

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI comboText;

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
        // Reset điểm về 0 mỗi khi màn chơi bắt đầu lại
        CurrentScore = 0;
        ComboCount = 0;

        UpdateScoreUI();
        UpdateComboUI();
    }

    public void AddPoints(int points)
    {
        CurrentScore += points;
        UpdateScoreUI();
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
}