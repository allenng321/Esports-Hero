using System.Collections.Generic;
using Game.Scripts.GameManagement;
using Game.Scripts.Objects.Rooms;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Scripts.Objects
{
    public class UpgradableItem : MonoBehaviour, IUpgradable
    {
        public RoomName itemRoom;
        public UpgradableName itemKey;

        public List<UpgradableObjectLevel> itemLevels;

        public int CurrentLevel { get; private set; }

        private AssetReferenceGameObject _currentLevelPrefab;
        private GameObject _instance;

        [SerializeField] private bool interactable = true;
        public bool Interactable => interactable;

        public bool UpgradeAvailable { get; private set; }

        public int NextLevelNumber { get; set; }
        public int UpgradeCost { get; set; }
        public UpgradeTime UpgradeTime { get; set; }
        private AssetReferenceSprite _nextLevelPreview;
        public Sprite Preview { get; set; } = null;

        private bool _init = false;

        private void Load(bool force = false)
        {
            if (_init && !force) return;
            _init = true;

            CurrentLevel = UpgradableLevelsData.UpgradablesData[itemKey];
            _currentLevelPrefab = itemLevels
                .Find(level => level.levelNumber == CurrentLevel).levelPrefab;

            var transform1 = transform;
            _currentLevelPrefab.InstantiateAsync(transform1.position, transform1.rotation, transform1).Completed +=
                handle => _instance = handle.Result;


            NextLevelNumber = CurrentLevel + 1;
            var nextLevel = itemLevels.Find(level => level.levelNumber == NextLevelNumber);
            UpgradeAvailable = !(nextLevel is null);

            if (nextLevel is null) return;

            UpgradeCost = nextLevel.upgradeCost;

            _nextLevelPreview = nextLevel.levelPreview;

            if (!(_nextLevelPreview is null) && _nextLevelPreview.IsValid())
            {
                _nextLevelPreview.LoadAssetAsync().Completed += handle =>
                {
                    if (handle.Result) Preview = handle.Result;
                };
            }
        }

        private void Awake() => Load();

        private void OnEnable() => Load();

        private void OnDestroy()
        {
            _currentLevelPrefab.ReleaseInstance(_instance);
            if (!(_nextLevelPreview is null) && _nextLevelPreview.IsValid()) _nextLevelPreview.ReleaseAsset();
        }

        public void FinishUpgrade()
        {
            //TODO: Play some animation, celebration effect etc. before just swapping the current level models with next
            UpgradableLevelsData.UpgradablesData[itemKey] = NextLevelNumber;
            UpgradableLevelsData.Save();
            Load(true);
        }
    }
}