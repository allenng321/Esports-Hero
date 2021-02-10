using System;
using System.Collections;
using System.Collections.Generic;
using Game.Scripts.GameManagement;
using Game.Scripts.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game.Scripts.Objects.Rooms
{
    [Serializable]
    public enum RoomName
    {
        Bedroom,
        Office
    }

    public class UpgradableRoom : MonoBehaviour, IUpgradable
    {
        [SerializeField] private RoomName roomName;
        public RoomName RoomName => roomName;
        [SerializeField] private UpgradableName roomKey;
        public SceneGroup thisRoomScenes;

        public List<UpgradableItem> RoomItems { get; private set; }

        public AssetReference roomLevelsData;

        private List<UpgradableObjectLevel> _roomLevels;

        private AssetReferenceGameObject _currentLevelPrefab;
        private GameObject _instance;

        public RoomName ObjectRoom => roomName;
        public UpgradableName ObjectKey => roomKey;
        public int CurrentLevelNumber { get; private set; }
        public int NextLevelNumber { get; private set; }
        public int NextLevelRequiredPlayerExpLevel { get; private set; }
        public int NextLevelPlayerExpProgressGrant { get; private set; }
        public bool NextLevelAvailable { get; private set; }
        public RequiredUpgrade[] NextLevelRequiredUpgrades { get; private set; }
        public int UpgradeCost { get; private set; }
        public UpgradeTime UpgradeTime { get; private set; }
        private AssetReferenceSprite _nextLevelPreview;
        public Sprite Preview { get; private set; }

        private bool _init = false;

        private void Load(bool force = false)
        {
            if (_init && !force) return;
            _init = true;

            roomLevelsData.LoadAssetAsync<UpgradableRoomLevels>().Completed += OnLevelsLoaded;
        }

        private IEnumerator RoomLoaded()
        {
            // Wait each frames till the UpgradableInteract prefab has been instantiated
            while (UpgradableItemsInteract.instance is null || !UpgradableItemsInteract.instance)
                yield return null;

            // A small wait to make sure everything is loaded before call.
            yield return new WaitForSeconds(1);

            RoomItems = new List<UpgradableItem>(GetComponentsInChildren<UpgradableItem>());

            foreach (var item in RoomItems) item.interactable = true;

            UpgradableItemsInteract.instance.RoomLoaded(roomName, RoomItems);
        }

        private void OnLevelsLoaded(AsyncOperationHandle<UpgradableRoomLevels> obj)
        {
            _roomLevels = obj.Result.roomLevels;

            if (!(_instance is null) && _instance != null && _instance)
            {
                _currentLevelPrefab.ReleaseInstance(_instance);
            }

            CurrentLevelNumber = UpgradableLevelsData.UpgradablesData[roomKey];
            _currentLevelPrefab = _roomLevels
                .Find(level => level.levelNumber == CurrentLevelNumber).levelPrefab;

            var transform1 = transform;
            _currentLevelPrefab.InstantiateAsync(transform1.position, transform1.rotation, transform1).Completed +=
                handle => _instance = handle.Result;

            StartCoroutine(RoomLoaded());


            NextLevelNumber = CurrentLevelNumber + 1;
            var nextLevel = _roomLevels.Find(level => level.levelNumber == NextLevelNumber);
            NextLevelAvailable = !(nextLevel is null);

            if (nextLevel is null) return;

            UpgradeCost = nextLevel.upgradeCost;
            UpgradeTime = nextLevel.upgradeTime;
            NextLevelRequiredUpgrades = nextLevel.requiredUpgrades;
            NextLevelRequiredPlayerExpLevel = nextLevel.requiredPlayerExpLevel;
            NextLevelPlayerExpProgressGrant = nextLevel.playerExpProgressGrant;

            _nextLevelPreview = nextLevel.levelPreview;

            if (!(_nextLevelPreview is null) && _nextLevelPreview.RuntimeKeyIsValid())
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
        }

        public void RoomChanging()
        {
            UpgradableItemsInteract.instance.ClearCurrentRoomUpgrades(roomName);
        }

        public void FinishUpgrade()
        {
            //TODO: Play some animation, celebration effect etc. before just swapping the current level prefab with next
            UpgradableLevelsData.UpgradablesData[roomKey] = NextLevelNumber;

            // Here also reset the room items to level 0 for the next room level.
            // SO that the next room begins with that room's respective starting items,
            // which an later be upgraded.
            foreach (var item in RoomItems)
            {
                if (UpgradableLevelsData.TryGetRunningUpgrade(item.ObjectKey, out var ud))
                {
                    UpgradableLevelsData.FinishRunningUpgrade(ud);
                    UpgradableItemsInteract.instance.StopUpgrade(ud);
                }

                UpgradableLevelsData.UpgradablesData[item.ObjectKey] = 1;
            }

            UpgradableLevelsData.Save();
            Load(true);
            PlayerManager.Instance.UpdatePlayerExpProgress(NextLevelPlayerExpProgressGrant);
        }
    }
}