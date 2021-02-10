using System;
using Game.Scripts.Objects.Rooms;
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
        [SerializeField] private PlayerGameDataObject playerGameDataObject;

        private GameRoom _currentRoom;

        private GameRoom CurrentRoom
        {
            get
            {
                if (!_currentRoom) _currentRoom = FindObjectOfType<GameRoom>();
                return _currentRoom;
            }
        }

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

        public string GetGameRankName() => playerGameDataObject.GetRankName(currentPlayerData.gameRank);

        public int GetNextRankProgress() =>
            playerGameDataObject.GetRequiredRankProgressForGoingToNextRank(currentPlayerData.gameRank + 1);

        public void UpdatePlayerExpProgress(int grant)
        {
            var pData = currentPlayerData;
            var expProgress = pData.playerExpProgress + grant;
            var requiredExpProgress =
                playerGameDataObject.GetRequiredExpProgressForReachingNextLevel(pData.playerExpLevel + 1);

            pData.playerExpProgress = expProgress;
            if (expProgress >= requiredExpProgress && requiredExpProgress > 0)
            {
                pData.playerExpLevel += 1;
                pData.playerExpProgress = expProgress - requiredExpProgress;
            }

            PlayerSaveData.CurrentData = pData;
            PlayerSaveData.Save();

            CurrentRoom.UpdateCanvass();
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
                $" Player Rank: {currentPlayerData.gameRank}, Rank Progress: {currentPlayerData.gameRankProgress}, Player Leaderboard Rating: {currentPlayerData.gameLeaderboardRating}," +
                $" Chance: {_chance}, Win: {_winCurrentSimulation}, Kills: {_kills}, Deaths: {_deaths}");
        }

        private void UpdatePlayerDataAfterGame()
        {
            var ratingIncrease = CalculateRatingChangeAfterGame();
            var coinsWon = CalculateCoinsWonOnGameWin();
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
            var kd = (float) _kills / _deaths;

            playerData.gameLeaderboardRating = Mathf.Clamp(playerData.gameLeaderboardRating - ratingIncrease, 1, 99999);

            var rp = playerData.gameRankProgress;
            var requiredRp = playerGameDataObject.GetRequiredRankProgressForGoingToNextRank(playerData.gameRank + 1);
            rp += Mathf.Clamp(ratingIncrease, 0, 25);
            if (_winCurrentSimulation)
            {
                var lbg = PlayerWinChanceCalculator.GetLeaderboardRatingGroup(playerData.gameLeaderboardRating);
                rp += IntRaisedToPower(2, 10 - lbg) + 5;
            }

            if (kd > 1)
            {
                var d = _kills - _deaths;
                rp += d * 2 + 1;
            }

            playerData.gameRankProgress = rp;
            if (requiredRp > 0 && rp > requiredRp)
            {
                playerData.gameRank += 1;
                playerData.gameRankProgress = rp - requiredRp;
            }

            playerData.coinsInWallet += coinsWon;

            PlayerSaveData.CurrentData = playerData;
            PlayerSaveData.Save();

            CurrentRoom.UpdateCanvass();
        }

        private static int IntRaisedToPower(int value, int power)
        {
            if (power <= 0) return 1;
            if (power == 1) return value;

            for (int i = 1; i < power; i++) value *= value;
            return value;
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

        // Calculates the amount of coins player gets after winning a multiplayer game battle depending on the leaderboard group player is in
        private int CalculateCoinsWonOnGameWin()
        {
            if (!_winCurrentSimulation) return 0;

            var lbg = PlayerWinChanceCalculator.GetLeaderboardRatingGroup(currentPlayerData.gameLeaderboardRating);
            switch (lbg)
            {
                case 1:
                    return 1250;
                case 2:
                    return 999;
                case 3:
                    return 890;
                case 4:
                    return 750;
                case 5:
                    return 500;
                case 6:
                    return 375;
                case 7:
                    return 250;
                case 8:
                    return 100;
                default:
                    return 0;
            }
        }
    }
}