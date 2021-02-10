using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.Scripts.GameManagement
{
    public struct PlayerData
    {
        public string playerName;
        [Range(1, int.MaxValue)] public int playerExpLevel;
        [Range(0, int.MaxValue)] public int playerExpProgress;
        [Range(0, int.MaxValue)] public int coinsInWallet, coinsInBank;
        public int consecutiveGameWins, totalGameWins, gameKills, gameDeaths;
        public int gameRank, gameRankProgress, gameLeaderboardRating;
        public SerializableDateTime lastGameTime;
    }

    public static class PlayerSaveData
    {
        private const long Version = 0;

        private static PlayerData _currentData;

        public static PlayerData CurrentData
        {
            get
            {
                if (!_inited) OnLoad();

                return _currentData;
            }
            set => _currentData = value;
        }

        public static PlayerData defaultData = new PlayerData
        {
            playerName = "Private Tester",
            playerExpLevel = 1,
            playerExpProgress = 0,
            coinsInWallet = 999999999,
            coinsInBank = 0,
            consecutiveGameWins = 0,
            totalGameWins = 0,
            gameKills = 0,
            gameDeaths = 0,
            gameRank = 1,
            gameRankProgress = 0,
            gameLeaderboardRating = 6000,
            lastGameTime = DateTime.UtcNow
        };

        private const string BinarySavePath = "PlayerData.sav";
        private static string _fullFilePath = "";

        private static bool _inited = false;

        [RuntimeInitializeOnLoadMethod]
        private static void OnLoad()
        {
            if (_inited) return;
            _fullFilePath = Path.Combine(Application.persistentDataPath, BinarySavePath);
            _inited = true;
            Load();
        }

        public static void Save()
        {
#if CSHARP_7_OR_LATER
            // A check to see c# version and if pass then run the write method on a different thread.
            // Used this to prevent any blocking of the game on main thread because of file system writes.
            // Also it is safe to do this on a separate thread since we are not using any of Unity's API(s) in Write method and use only System namespace types.
            var task = new Task(InternalSave);
            task.Start();
#else
            InternalSave();
#endif
        }

        private static void InternalSave()
        {
            if (File.Exists(_fullFilePath))
            {
                // well there are too many `bkp`s usage here 😅
                if (File.Exists(_fullFilePath + ".bkp"))
                    File.Replace(_fullFilePath, _fullFilePath + ".bkp", _fullFilePath + ".bkp.bkp");
                else File.Move(_fullFilePath, _fullFilePath + ".bkp");
            }

            using (BinaryWriter writer = new BinaryWriter(File.Open(_fullFilePath, FileMode.CreateNew)))
            {
                writer.Write(Version);
                writer.Write(CurrentData.playerName);
                writer.Write(CurrentData.playerExpLevel);
                writer.Write(CurrentData.playerExpProgress);
                writer.Write(CurrentData.coinsInWallet);
                writer.Write(CurrentData.coinsInBank);
                writer.Write(CurrentData.consecutiveGameWins);
                writer.Write(CurrentData.totalGameWins);
                writer.Write(CurrentData.gameKills);
                writer.Write(CurrentData.gameDeaths);
                writer.Write(CurrentData.gameRank);
                writer.Write(CurrentData.gameRankProgress);
                writer.Write(CurrentData.gameLeaderboardRating);
                writer.Write(JsonUtility.ToJson(CurrentData.lastGameTime));
            }
        }

        public static bool SaveExists() => File.Exists(_fullFilePath) || File.Exists(_fullFilePath + ".bkp");

        public static void Load()
        {
            if (!TryLoadFrom(_fullFilePath))
                TryLoadFrom(_fullFilePath + ".bkp");
        }

        private static bool TryLoadFrom(string path)
        {
            _currentData = defaultData;

            try
            {
                if (File.Exists(path))
                {
                    using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
                    {
                        var savedVersion = reader.ReadInt64();

                        if (savedVersion == Version)
                        {
                            _currentData = new PlayerData
                            {
                                playerName = reader.ReadString(),
                                playerExpLevel = reader.ReadInt32(),
                                playerExpProgress = reader.ReadInt32(),
                                coinsInWallet = reader.ReadInt32(),
                                coinsInBank = reader.ReadInt32(),
                                consecutiveGameWins = reader.ReadInt32(),
                                totalGameWins = reader.ReadInt32(),
                                gameKills = reader.ReadInt32(),
                                gameDeaths = reader.ReadInt32(),
                                gameRank = reader.ReadInt32(),
                                gameRankProgress = reader.ReadInt32(),
                                gameLeaderboardRating = reader.ReadInt32(),
                                lastGameTime = JsonUtility.FromJson<SerializableDateTime>(reader.ReadString())
                            };
                        }
                        else
                        {
                            Debug.Log(
                                $"Player save file:{path} very old, saved version:{savedVersion}, deleting it.");
                            File.Delete(path);
                        }

                        return true;
                    }
                }
            }
            catch (Exception)
            {
                Debug.Log($"Player save:{path} damaged.");
                File.Delete(path);
            }

            return false;
        }
    }
}