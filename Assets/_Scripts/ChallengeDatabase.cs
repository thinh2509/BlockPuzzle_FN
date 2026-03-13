using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._Scripts
{
    [CreateAssetMenu(fileName = "ChallengeDatabase", menuName = "Challenge/Database")]
    public class ChallengeDatabase : ScriptableObject
    {
        public List<ChallengeLevelData> levels = new List<ChallengeLevelData>();

        public List<ChallengeLevelData> GetLevelsByMode(ChallengeModeType mode)
        {
            return levels
                .Where(x => x != null && x.mode == mode)
                .OrderBy(x => x.levelNumber)
                .ToList();
        }

        public ChallengeLevelData GetNextLevel(ChallengeLevelData current)
        {
            if (current == null) return null;

            var sameMode = GetLevelsByMode(current.mode);
            var nextInMode = sameMode.FirstOrDefault(x => x.levelNumber == current.levelNumber + 1);
            if (nextInMode != null) return nextInMode;

            int nextModeInt = (int)current.mode + 1;

            if (!System.Enum.IsDefined(typeof(ChallengeModeType), nextModeInt))
                return null;

            ChallengeModeType nextMode = (ChallengeModeType)nextModeInt;
            return GetLevelsByMode(nextMode).FirstOrDefault();
        }
    }
}
