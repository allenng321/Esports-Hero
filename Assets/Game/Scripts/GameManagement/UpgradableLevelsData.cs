using System;
using System.Collections.Generic;
using System.IO;
using Game.Scripts.Objects.Rooms;
using UnityEngine;

namespace Game.Scripts.GameManagement
{
    public static class UpgradableLevelsData
    {
        private const long Version = 0;

        private static Dictionary<UpgradableName, int> _upgradablesData;

        public static Dictionary<UpgradableName, int> UpgradablesData
        {
            get
            {
                if (_upgradablesData is null) OnLoad();

                return _upgradablesData;
            }
        }

        private static Dictionary<UpgradableName, int> DefaultUpgradablesData =>
            new Dictionary<UpgradableName, int>
            {
                [UpgradableName.BedroomLevel] = 1,
                [UpgradableName.BedroomBedLevel] = 1,
                [UpgradableName.BedroomPCLevel] = 1,
                [UpgradableName.BedroomFurnitureLevel] = 1
            };

        private static List<UpgradeData> _upgrades = null;
        public static Dictionary<RoomName, List<UpgradeData>> upgrades = new Dictionary<RoomName, List<UpgradeData>>();

        private const string BinarySavePath = "UpgradableItemLevels.sav";
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
            using (BinaryWriter writer = new BinaryWriter(File.Open(_fullFilePath, FileMode.CreateNew)))
            {
                writer.Write(Version);
                writer.Write(JsonUtility.ToJson(new RunningUpgrades(_upgrades.ToArray()), false));
                writer.Write(_upgradablesData[UpgradableName.BedroomLevel]);
                writer.Write(_upgradablesData[UpgradableName.BedroomBedLevel]);
                writer.Write(_upgradablesData[UpgradableName.BedroomPCLevel]);
                writer.Write(_upgradablesData[UpgradableName.BedroomFurnitureLevel]);
            }
        }

        public static void Load()
        {
            if (!TryLoadFrom(_fullFilePath))
                TryLoadFrom(_fullFilePath + ".bkp");

            ProcessSavedRunningUpgrades();
        }

        private static bool TryLoadFrom(string path)
        {
            _upgradablesData = DefaultUpgradablesData;

            try
            {
                if (File.Exists(path))
                {
                    using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
                    {
                        var savedVersion = reader.ReadInt64();

                        if (savedVersion == Version)
                        {
                            _upgrades = new List<UpgradeData>(JsonUtility.FromJson<RunningUpgrades>(reader.ReadString())
                                .upgrades);
                            _upgradablesData[UpgradableName.BedroomLevel] = reader.ReadInt32();
                            _upgradablesData[UpgradableName.BedroomBedLevel] = reader.ReadInt32();
                            _upgradablesData[UpgradableName.BedroomPCLevel] = reader.ReadInt32();
                            _upgradablesData[UpgradableName.BedroomFurnitureLevel] = reader.ReadInt32();
                        }
                        else
                        {
                            Debug.Log(
                                $"Upgradables data file:{path} very old, saved version:{savedVersion}, deleting it.");
                            File.Delete(path);
                        }

                        return true;
                    }
                }
            }
            catch (Exception)
            {
                Debug.Log($"Upgradables data:{path} damaged.");
                File.Delete(path);
            }

            return false;
        }

        private static void CreatePublicUpgradesDict()
        {
            foreach (var v in Enum.GetValues(typeof(RoomName)))
            {
                if (v.GetType() == typeof(RoomName) && v is RoomName o)
                    upgrades[o] = new List<UpgradeData>();
            }
        }

        private static void ProcessSavedRunningUpgrades()
        {
            CreatePublicUpgradesDict();

            if (_upgrades is null) return;

            var timeNow = DateTime.UtcNow;

            for (int i = 0; i < _upgrades.Count; i++)
            {
                var u = _upgrades[i];
                if (timeNow > u.endTime)
                {
                    _upgradablesData[u.item] = u.upgradedVersion;
                    _upgrades.Remove(u);
                }
                else upgrades[u.room].Add(u);
            }
        }

        public static void StartNewUpgrade(UpgradeData data)
        {
            _upgrades.Add(data);
            upgrades[data.room].Add(data);
        }

        public static void FinishRunningUpgrade(UpgradeData data)
        {
            if (_upgrades.Contains(data)) _upgrades.Remove(data);
            if (upgrades[data.room].Contains(data)) upgrades[data.room].Remove(data);
        }
    }
}