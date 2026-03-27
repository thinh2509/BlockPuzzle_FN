using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._Scripts
{
    public class ChallengeLevelSelectUI : MonoBehaviour
    {
        [SerializeField] private ChallengeDatabase database;
        [SerializeField] private Transform contentRoot;
        [SerializeField] private ChallengeLevelButton buttonPrefab;
        [SerializeField] private ChallengeModeType currentMode;

        private void Start()
        {
            Build();
        }

        public void Build()
        {
            foreach (Transform child in contentRoot)
                Destroy(child.gameObject);

            if (database == null || contentRoot == null || buttonPrefab == null)
                return;

            var levels = database.GetLevelsByMode(currentMode);

            foreach (var level in levels)
            {
                var btn = Instantiate(buttonPrefab, contentRoot);
                btn.Setup(level, database);
            }
        }

        public void ShowMode(int modeIndex)
        {
            currentMode = (ChallengeModeType)modeIndex;
            Build();
        }
    }
}
