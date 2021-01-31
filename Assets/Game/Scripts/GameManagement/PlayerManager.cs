using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Game.Scripts.GameManagement
{
    public class PlayerManager : MonoBehaviour
    {
        private const string PlayerManagerPrefabText = "PlayerManagerPrefab";

        public static PlayerManager Instance { get; private set; }
        public static bool isUsingPC;

        private PlayerData currentPlayerData => PlayerSaveData.CurrentData;

        private bool _winCurrentSimulation;
        private int _chance, _kills, _deaths;

        private DateTime _lastPlayTime;
        private const int MatchMinutes = 1;
        private int _currentMatchTime;

        [SerializeField] private GameObject playingGameUIPanel;
        [SerializeField] private Text gameTimeRemainingText;

        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeOnLoad()
        {
            if (!(Instance is null)) return;

            Addressables.InstantiateAsync(PlayerManagerPrefabText);
        }

        private void OnEnable()
        {
            if (!(Instance is null)) Destroy(gameObject);
            else
            {
#if UNITY_EDITOR
                Application.targetFrameRate = 60;
#endif
                Instance = this;
                DontDestroyOnLoad(Instance);
                playingGameUIPanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (!isUsingPC) return;

            var now = DateTime.UtcNow;
            var gameTimeLeft = now - _lastPlayTime;
            gameTimeRemainingText.text = _currentMatchTime - (int) gameTimeLeft.TotalMinutes + "m left";

            if (gameTimeLeft.Minutes >= _currentMatchTime) EndGameSimulation();
        }

        public void RunGameSimulation()
        {
            UpdatePlayerDataBeforeGame();

            _currentMatchTime = MatchMinutes;
            _chance = PlayerWinChanceCalculator.CalculateWinChance(out _kills, out _deaths);
            _winCurrentSimulation = false;

            if (_chance > 75)
                // definite win
                _winCurrentSimulation = true;
            else if (_chance > 50)
                // 1 out of 3 chance to win but some extra chance to increase chance if player is above rank 5
                _winCurrentSimulation = Random.Range(0,
                    3 + currentPlayerData.gameRank > 5
                        ? Random.Range(0, 2 + currentPlayerData.gameRank > 12 ? 1 : 0)
                        : 0) > 1;
            else if (_chance == 50)
                // 1 out of 2 chance to win but some extra chance to increase chance if player is above rank 8 and player has won more than 2 match consecutively right before
                _winCurrentSimulation = Random.Range(0,
                    2 + currentPlayerData.gameRank > 8 && currentPlayerData.consecutiveGameWins > 2
                        ? Random.Range(0, 2 + currentPlayerData.gameRank > 17 ? 1 : 0)
                        : 0) > 1;
            else if (_chance > 37)
                // high chance of loss with increased win chance if player rank is high
                _winCurrentSimulation = Random.Range(-2,
                    3 + currentPlayerData.gameRank > 12
                        ? Random.Range(0, 2 + currentPlayerData.gameRank > 22 ? currentPlayerData.gameRank - 22 : 0)
                        : 0) > 1;
            else if (_chance > 25)
                // 1 out of 3 chance to win but some extra chance to increase chance if player is above rank 5
                _winCurrentSimulation = Random.Range(-4,
                    3 + currentPlayerData.gameRank > 15
                        ? Random.Range(0, 2 + currentPlayerData.gameRank > 25 ? currentPlayerData.gameRank - 25 : 0)
                        : 0) > 1;
            else if (_chance > 11)
                // 1 out of 3 chance to win but some extra chance to increase chance if player is above rank 5
                _winCurrentSimulation = Random.Range(-6,
                    3 + currentPlayerData.gameRank > 18
                        ? Random.Range(0, 2 + currentPlayerData.gameRank > 25 ? currentPlayerData.gameRank - 25 : 0)
                        : 0) > 1;
            else
                // definite loss
                _winCurrentSimulation = false;

            if (currentPlayerData.gameLeaderboardRating < 100) _currentMatchTime += 10;
            else if (currentPlayerData.gameLeaderboardRating < 1000) _currentMatchTime += 5;

            isUsingPC = true;
            playingGameUIPanel.SetActive(true);
        }

        private void UpdatePlayerDataBeforeGame()
        {
            var playerDat = currentPlayerData;
            playerDat.lastGameTime = _lastPlayTime = DateTime.UtcNow;
            PlayerSaveData.CurrentData = playerDat;
            PlayerSaveData.Save();
        }

        private void EndGameSimulation()
        {
            isUsingPC = false;
            playingGameUIPanel.SetActive(false);

            UpdatePlayerDataAfterGame();

            Debug.Log(
                $"Consecutive Wins: {currentPlayerData.consecutiveGameWins}, Total Wins: {currentPlayerData.totalGameWins}," +
                $" Player Rank: {currentPlayerData.gameRank}, Player Leaderboard Rating: {currentPlayerData.gameLeaderboardRating}," +
                $" Chance: {_chance}, Win: {_winCurrentSimulation}, Kills: {_kills}, Deaths: {_deaths}");
        }

        private void UpdatePlayerDataAfterGame()
        {
            var ratingIncrease = CalculateRatingChangeAfterGame();
            var playerData = currentPlayerData;

            if (_winCurrentSimulation)
            {
                if (playerData.consecutiveGameWins < 0) playerData.consecutiveGameWins = 1;
                else playerData.consecutiveGameWins += 1;
                playerData.totalGameWins += 1;
            }
            else
            {
                if (playerData.consecutiveGameWins > 0) playerData.consecutiveGameWins = -1;
                else playerData.consecutiveGameWins -= 1;
            }

            playerData.gameKills += _kills;
            playerData.gameDeaths += _deaths;
            playerData.gameLeaderboardRating = Mathf.Clamp(playerData.gameLeaderboardRating - ratingIncrease, 1, 99999);

            playerData.gameRankProgress += Mathf.Clamp(ratingIncrease, 0, 75);
            if (playerData.gameRankProgress > 1250)
            {
                playerData.gameRank += 1;
                playerData.gameRankProgress -= 1250;
            }

            PlayerSaveData.CurrentData = playerData;
            PlayerSaveData.Save();
        }

        private int CalculateRatingChangeAfterGame()
        {
            var delta = _winCurrentSimulation ? 15 : -15;

            if (_winCurrentSimulation)
            {
                var kd = (float) _kills / _deaths;
                if (kd > 4) delta += Random.Range(2, 5);
                else if (kd > 1.01) delta += 2;
                else if (kd < .9f) delta -= 2;

                if (_chance < 25) delta += Random.Range(3, 11);
                else if (_chance < 35) delta += Random.Range(3, 7);
                else if (_chance < 50) delta += Random.Range(2, 6);
            }
            else
            {
                if (_chance > 90) delta -= Random.Range(3, 10);
                else if (_chance > 75) delta -= Random.Range(2, 7);
                else if (_chance > 65) delta -= Random.Range(2, 5);
            }

            return delta;
        }
    }
}