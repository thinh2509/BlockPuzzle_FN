using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._Scripts
{

    public static class ChallengeProgressService
    {
        private const string CompletedPrefix = "challenge_completed_";
        private const string UnlockedModeKey = "challenge_unlocked_mode";

        public static bool IsLevelCompleted(string levelId)
        {
            return PlayerPrefs.GetInt(CompletedPrefix + levelId, 0) == 1;
        }

        public static void CompleteLevel(ChallengeLevelData levelData, ChallengeDatabase db)
        {
            PlayerPrefs.SetInt(CompletedPrefix + levelData.levelId, 1);

            int currentUnlockedMode = PlayerPrefs.GetInt(UnlockedModeKey, 0);

            var sameModeLevels = db.GetLevelsByMode(levelData.mode);
            bool allDone = true;

            foreach (var level in sameModeLevels)
            {
                if (!IsLevelCompleted(level.levelId))
                {
                    allDone = false;
                    break;
                }
            }

            if (allDone)
            {
                int nextMode = Mathf.Max(currentUnlockedMode, (int)levelData.mode + 1);
                PlayerPrefs.SetInt(UnlockedModeKey, nextMode);
            }

            PlayerPrefs.Save();
        }

        public static bool IsModeUnlocked(ChallengeModeType mode)
        {
            int unlockedMode = PlayerPrefs.GetInt(UnlockedModeKey, 0);
            return (int)mode <= unlockedMode;
        }

        public static bool IsLevelUnlocked(ChallengeLevelData levelData, ChallengeDatabase db)
        {
            if (!IsModeUnlocked(levelData.mode))
                return false;

            var levels = db.GetLevelsByMode(levelData.mode);
            if (levels.Count == 0)
                return false;

            if (levels[0] == levelData)
                return true;

            ChallengeLevelData previous = levels.Find(x => x.levelNumber == levelData.levelNumber - 1);
            return previous != null && IsLevelCompleted(previous.levelId);
        }

        public static void ResetProgress()
        {
            PlayerPrefs.DeleteKey(UnlockedModeKey);
            PlayerPrefs.Save();
        }
    }
}
