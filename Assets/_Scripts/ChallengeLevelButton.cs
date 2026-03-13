using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Assets._Scripts
{


    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.SceneManagement;

    public class ChallengeLevelButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Button button;

        private ChallengeLevelData levelData;
        private ChallengeDatabase database;

        public void Setup(ChallengeLevelData level, ChallengeDatabase db)
        {
            levelData = level;
            database = db;

            bool unlocked = ChallengeProgressService.IsLevelUnlocked(levelData, database);
            bool completed = ChallengeProgressService.IsLevelCompleted(levelData.levelId);

            if (titleText != null)
                titleText.text = levelData.displayName;

            if (statusText != null)
                statusText.text = completed ? "Completed" : (unlocked ? "Unlocked" : "Locked");

            if (button != null)
            {
                button.interactable = unlocked;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnClickPlay);
            }
        }

        private void OnClickPlay()
        {
            ChallengeSession.SelectedLevel = levelData;
            SceneManager.LoadScene("Challenge");
        }
    }
}
