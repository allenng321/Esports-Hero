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

    public class UpgradableRoom : MonoBehaviour
    {
        public RoomName roomName;
        public UpgradableName roomKey;

        public AssetReference roomLevelsData;

        private List<UpgradableObjectLevel> _roomLevels;

        public int CurrentLevel { get; private set; }

        private AssetReferenceGameObject _currentLevelPrefab;
        private GameObject _instance;

        private bool _init = false;

        private void Load(bool force = false)
        {
            if (_init && !force) return;
            _init = true;

            roomLevelsData.LoadAssetAsync<UpgradableRoomLevels>().Completed += OnLevelsLoaded;

            StartCoroutine(RoomLoaded());
        }

        private IEnumerator RoomLoaded()
        {
            while (UpgradableItemsInteract.instance is null || !UpgradableItemsInteract.instance)
                yield return null;

            UpgradableItemsInteract.instance.RoomLoaded(roomName);
        }

        private void OnLevelsLoaded(AsyncOperationHandle<UpgradableRoomLevels> obj)
        {
            _roomLevels = obj.Result.roomLevels;

            CurrentLevel = UpgradableLevelsData.UpgradablesData[roomKey];
            _currentLevelPrefab = _roomLevels
                .Find(level => level.levelNumber == CurrentLevel).levelPrefab;

            var transform1 = transform;
            _currentLevelPrefab.InstantiateAsync(transform1.position, transform1.rotation, transform1).Completed +=
                handle => _instance = handle.Result;
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
    }
}