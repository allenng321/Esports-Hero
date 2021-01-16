using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
// using System.Threading;
using System.Threading.Tasks;
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

        private static List<UpgradeData> _runningUpgrades;

        public static readonly Dictionary<RoomName, List<UpgradeData>> RunningUpgrades =
            new Dictionary<RoomName, List<UpgradeData>>();

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
#if CSHARP_7_OR_LATER
            // A check to see c# version and if pass then run the write method on a different thread.
            // Used this to prevent any blocking of the game on main thread because of file system writes.
            // Also it is safe to do this on a separate thread since we are not using any of Unity's API(s) in Write method and use only System namespace types.
            var task = new Task(InternalSave);
            task.Start();
            // var t = new Thread(InternalSave);
            // t.Start();
#else
            var chars = Convert
                .ToBase64String(
                    Encoding.UTF8.GetBytes(JsonUtility.ToJson(new RunningUpgrades(_runningUpgrades.ToArray()),
                        false))).ToCharArray();

            Write(_fullFilePath, chars);
            Write(_fullFilePath + ".bkp", chars);
#endif
        }

        private static void InternalSave()
        {
            // The following actually just converts string to byte[] and then Base64Encode it.
            // The saved data is the resulting string after Base64Encode, the convert to char[] however is only done to keep track of characters in the encoded string.
            // So as to use the number at reading time for verification and securing upgradable level data and game integrity.
            //
            // If someone actually went through all the hustle, opened the file and decoded Base64 then modified the json string value of UpgradesData,
            // and saved it again to gain advantage in game by cheating the save file. At reading the save file will simply be discarded as corrupt because the
            // character numbers do not match with what it was at saving time, so ultimately killing all hopes of the cheater and reverting back to the original saved data.
            var chars = Convert
                .ToBase64String(
                    Encoding.UTF8.GetBytes(JsonUtility.ToJson(new RunningUpgrades(_runningUpgrades.ToArray()),
                        false))).ToCharArray();

            Write(_fullFilePath, chars);
            Write(_fullFilePath + ".bkp", chars);
        }

        private static void Write(string file, char[] chars)
        {
            // Debugs here are necessary to let us know what is happening on the separate thread
            try
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(file, FileMode.OpenOrCreate)))
                {
                    Console.WriteLine("ULD writer entered");
                    writer.Write(Version);
                    writer.Write(chars.Length);
                    writer.Write(chars);
                    writer.Write(_upgradablesData[UpgradableName.BedroomLevel]);
                    writer.Write(_upgradablesData[UpgradableName.BedroomBedLevel]);
                    writer.Write(_upgradablesData[UpgradableName.BedroomPCLevel]);
                    writer.Write(_upgradablesData[UpgradableName.BedroomFurnitureLevel]);
                    Console.WriteLine("ULD writer written all data");
                }

                Console.WriteLine("ULD writer exited");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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
                            var chars = reader.ReadInt32();
                            _runningUpgrades = new List<UpgradeData>(JsonUtility
                                .FromJson<RunningUpgrades>(
                                    Encoding.UTF8.GetString(
                                        Convert.FromBase64String(new string(reader.ReadChars(chars))))).upgrades);
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
                    RunningUpgrades[o] = new List<UpgradeData>();
            }
        }

        private static void ProcessSavedRunningUpgrades()
        {
            CreatePublicUpgradesDict();

            if (_runningUpgrades is null) _runningUpgrades = new List<UpgradeData>();

            var timeNow = DateTime.UtcNow;

            for (int i = 0; i < _runningUpgrades.Count; i++)
            {
                var u = _runningUpgrades[i];
                if (timeNow > u.endTime)
                {
                    _upgradablesData[u.item] = u.upgradedVersion;
                    _runningUpgrades.Remove(u);
                }
                else RunningUpgrades[u.room].Add(u);
            }
        }

        public static void StartNewUpgrade(UpgradeData data)
        {
            _runningUpgrades.Add(data);
            RunningUpgrades[data.room].Add(data);
        }

        public static void FinishRunningUpgrade(UpgradeData data)
        {
            if (_runningUpgrades.Contains(data)) _runningUpgrades.Remove(data);
            if (RunningUpgrades[data.room].Contains(data)) RunningUpgrades[data.room].Remove(data);
        }

        public static bool TryGetRunningUpgrade(UpgradableName key, out UpgradeData ud)
        {
            ud = _runningUpgrades.Find(data => data.item == key);
            if (ud != null) return true;

            ud = null;
            return false;
        }
    }
}