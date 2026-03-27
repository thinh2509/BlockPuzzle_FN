using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._Scripts
{

    [CreateAssetMenu(fileName = "ChallengeLevel_", menuName = "Challenge/Level Data")]
    public class ChallengeLevelData : ScriptableObject
    {
        [Header("Identity")]
        public string levelId;
        public string displayName;
        public ChallengeModeType mode;
        public int levelNumber;

        [Header("Rules")]
        public int targetScore;
        public float timeLimit;
        public int moveLimit;
        public int obstacleCount;

        public int targetPointsForLimitedMoves = 500;  
        public int maxMovesForLimitedMoves = 10;     
        public bool isTargetPointsEnabled = true;     
        public int bonusPointsForTarget = 100;

        [TextArea]
        public string description;
    }
}

