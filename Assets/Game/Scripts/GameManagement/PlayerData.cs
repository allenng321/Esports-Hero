﻿using System;
using System.IO;
using UnityEngine;

namespace Game.Scripts.GameManagement
{
    public struct PlayerData
    {
        public string playerName;
        [Range(1, int.MaxValue)] public int playerExp;
        [Range(0, int.MaxValue)] public int coinsInWallet, coinsInBank;
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
            playerExp = 1,
            coinsInWallet = 999999999,
            coinsInBank = 0
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
                writer.Write(CurrentData.playerExp);
                writer.Write(CurrentData.coinsInWallet);
                writer.Write(CurrentData.coinsInBank);
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
                                playerExp = reader.ReadInt32(),
                                coinsInWallet = reader.ReadInt32(),
                                coinsInBank = reader.ReadInt32()
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