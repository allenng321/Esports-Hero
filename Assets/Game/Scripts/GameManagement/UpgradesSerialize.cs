using System;
using Game.Scripts.Objects.Rooms;

namespace Game.Scripts.GameManagement
{
    [Serializable]
    public enum UpgradableName
    {
        BedroomLevel,
        BedroomBedLevel,
        BedroomPCLevel,
        BedroomFurnitureLevel
    }

    [Serializable]
    public struct SerializableDateTime
    {
        public int second;
        public int minute;
        public int hour;

        public int day;
        public int month;
        public int year;

        public static implicit operator DateTime(SerializableDateTime jdt) => new DateTime(year: jdt.year,
            month: jdt.month, day: jdt.day, hour: jdt.hour, minute: jdt.minute, second: jdt.second);

        public static implicit operator SerializableDateTime(DateTime dt) => new SerializableDateTime
        {
            year = dt.Year,
            month = dt.Month,
            day = dt.Day,
            hour = dt.Hour,
            minute = dt.Minute,
            second = dt.Second
        };
    }

    [Serializable]
    public class UpgradeData
    {
        public RoomName room;
        public UpgradableName item;
        public int upgradedVersion;
        public SerializableDateTime startTime;
        public SerializableDateTime endTime;
    }

    [Serializable]
    public struct RunningUpgrades
    {
        public UpgradeData[] upgrades;

        public RunningUpgrades(UpgradeData[] value)
        {
            upgrades = value;
        }
    }
}