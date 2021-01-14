using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Scripts.Objects
{
    public interface IUpgradable
    {
        int NextLevelNumber { get; set; }
        int UpgradeCost { get; set; }
        UpgradeTime UpgradeTime { get; set; }
        Sprite Preview { get; set; }

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
            return $"{days}d {hours}h {minutes}m {seconds}s";
        }
    }

    [Serializable]
    public class UpgradableObjectLevel
    {
        public int levelNumber;
        public int upgradeCost;
        public UpgradeTime upgradeTime;
        public AssetReferenceGameObject levelPrefab;
        public AssetReferenceSprite levelPreview;
    }
}