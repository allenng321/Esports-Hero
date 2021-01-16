using System;
using Game.Scripts.GameManagement;
using Game.Scripts.Objects.Rooms;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Scripts.Objects
{
    public interface IUpgradable
    {
        RoomName ObjectRoom { get; }
        UpgradableName ObjectKey { get; }
        int CurrentLevelNumber { get; }
        int NextLevelNumber { get; }
        bool NextLevelAvailable { get; }
        RequiredUpgrade[] NextLevelRequiredUpgrades { get; }
        int UpgradeCost { get; }
        UpgradeTime UpgradeTime { get; }
        Sprite Preview { get; }

        void FinishUpgrade();
    }

    [Serializable]
    public class UpgradeTime
    {
        public int days = 0, hours = 0, minutes = 0, seconds = 0;

        public static implicit operator TimeSpan(UpgradeTime ut) =>
            new TimeSpan(days: ut.days, hours: ut.hours, minutes: ut.minutes, seconds: ut.seconds);

        public static implicit operator UpgradeTime(TimeSpan ts) => new UpgradeTime
        {
            days = ts.Days,
            hours = ts.Hours,
            minutes = ts.Minutes,
            seconds = ts.Seconds
        };

        public override string ToString()
        {
            return days + hours + minutes + seconds == 0 ? "Instantaneous" : $"{days}d {hours}h {minutes}m {seconds}s";
        }
    }

    [Serializable]
    public class RequiredUpgrade
    {
        public UpgradableName item;
        public int levelRequired;
    }

    [Serializable]
    public class UpgradableObjectLevel
    {
        public int levelNumber;
        public int upgradeCost;
        public UpgradeTime upgradeTime;
        public AssetReferenceGameObject levelPrefab;
        public AssetReferenceSprite levelPreview;
        public RequiredUpgrade[] requiredUpgrades;
    }
}