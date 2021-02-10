using System.Globalization;
using Game.Scripts.GameManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Objects.Items
{
    public class PC : InteractableItemCanvass
    {
        [SerializeField] private GameObject mainPanel, gameRunningPanel, gameStatsPanel;

        [SerializeField] private Text gameRank, gameLeaderboardRating, gameKillsPerDeaths;

        [SerializeField] private Slider nextRankProgress;

        protected override void OnEnable()
        {
            base.OnEnable();
            mainPanel.SetActive(true);
            if (PlayerManager.isUsingPC)
            {
                mainPanel.SetActive(false);
                gameRunningPanel.SetActive(true);
            }

            var d = PlayerSaveData.CurrentData;
            gameRank.text = PlayerManager.Instance.GetGameRankName();
            gameLeaderboardRating.text = d.gameLeaderboardRating.ToString();
            gameKillsPerDeaths.text = ((float) d.gameKills / d.gameDeaths).ToString(CultureInfo.CurrentCulture);
            nextRankProgress.maxValue = PlayerManager.Instance.GetNextRankProgress();
            nextRankProgress.value = PlayerSaveData.CurrentData.gameRankProgress;
        }

        public override void ExitCanvass()
        {
            gameRunningPanel.SetActive(false);
            gameStatsPanel.SetActive(false);
            base.ExitCanvass();
        }

        public void PlayGame()
        {
            PlayerManager.Instance.RunGameSimulation();
            ExitCanvass();
        }

        public void LiveStreamGame()
        {
            PlayGame();
            ExitCanvass();
        }

        public void CheckGameStats()
        {
            mainPanel.SetActive(false);
            gameStatsPanel.SetActive(true);
        }
    }
}