using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts
{
    

    public class ResultUIManager : MonoBehaviour
    {
        public static ResultUIManager Instance;

        [Header("UI Panels")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject losePanel;

        [Header("Score Texts")]
        [SerializeField] private TMP_Text winScoreText;
        [SerializeField] private TMP_Text loseScoreText;

        void Awake()
        {
            Instance = this;
            HideAll();
        }

        public void HideAll()
        {
            if (resultPanel != null) resultPanel.SetActive(false);
            if (winPanel != null) winPanel.SetActive(false);
            if (losePanel != null) losePanel.SetActive(false);

        }

        public void ShowWin()
        {
            if (resultPanel != null) resultPanel.SetActive(true);
            if (winPanel != null)
                winPanel.SetActive(true);
            if (losePanel != null) losePanel.SetActive(false);

            int score = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;

            if (winScoreText != null)
                winScoreText.text =
     $"<color=#FFD700>SCORE</color>: <color=white>{score}</color>";

        }

        public void ShowLose()
        {
            if (resultPanel != null) resultPanel.SetActive(true);
            if (winPanel != null) winPanel.SetActive(false);
            if (losePanel != null) losePanel.SetActive(true);

            int score = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;

            if (loseScoreText != null)
                loseScoreText.text =
    $"<color=#FF5555>SCORE</color>: <color=white>{score}</color>";
        }
    }
}
