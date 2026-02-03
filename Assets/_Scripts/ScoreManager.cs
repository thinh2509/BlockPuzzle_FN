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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
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

    //private void UpdateScoreUI()
    //{
    //    if (scoreText != null)
    //    {
    //        scoreText.text = "Score: " + CurrentScore.ToString();
    //    }
    //    else
    //    {
    //        Debug.LogWarning("Score TextMeshProUGUI is not assigned in ScoreManager.");
    //    }
    //}
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {         
            scoreText.text = "<color=#55AAFF>S</color><color=#FF5555>C</color><color=#FFAA00>O</color><color=#55FF55>R</color><color=#FFDD55>E</color>: " + CurrentScore.ToString();
        }
        else
        {
            UnityEngine.Debug.LogWarning("Score TextMeshProUGUI is not assigned in ScoreManager.");
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
                comboText.text = null;
            }
        }
        else
        {
            Debug.LogWarning("Combo TextMeshProUGUI is not assigned in ScoreManager.");
        }
    }
}
