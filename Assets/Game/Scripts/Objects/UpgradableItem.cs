using System.Collections.Generic;
using Game.Scripts.GameManagement;
using Game.Scripts.Objects.Rooms;
using Game.Scripts.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;

namespace Game.Scripts.Objects
{
    public sealed class UpgradableItem : MonoBehaviour, IUpgradable, IPointerClickHandler
    {
        [SerializeField] private RoomName itemRoom;
        [SerializeField] private UpgradableName itemKey;

        [SerializeField] private List<UpgradableObjectLevel> itemLevels;
        [SerializeField] private GameObject interactionCanvass;

        private AssetReferenceGameObject _currentLevelPrefab;
        private GameObject _instance;

        // This should be false during first second of scene load until the room sets it to be interactable
        [HideInInspector] public bool interactable;

        public RoomName ObjectRoom => itemRoom;
        public UpgradableName ObjectKey => itemKey;
        public int CurrentLevelNumber { get; private set; }
        public int NextLevelNumber { get; private set; }
        public bool NextLevelAvailable { get; private set; }
        public RequiredUpgrade[] NextLevelRequiredUpgrades { get; private set; }
        public int UpgradeCost { get; private set; }
        public UpgradeTime UpgradeTime { get; private set; }
        private AssetReferenceSprite _nextLevelPreview;
        public Sprite Preview { get; private set; }

        private bool _init;

        public UpgradableItem(UpgradableName itemKey)
        {
            this.itemKey = itemKey;
        }

        private void Load(bool force = false)
        {
            if (_init && !force) return;
            _init = true;

            interactionCanvass.SetActive(false);

            if (!(_instance is null) && _instance != null && _instance)
            {
                _currentLevelPrefab.ReleaseInstance(_instance);
            }

            CurrentLevelNumber = UpgradableLevelsData.UpgradablesData[itemKey];
            _currentLevelPrefab = itemLevels
                .Find(level => level.levelNumber == CurrentLevelNumber).levelPrefab;

            var transform1 = transform;
            _currentLevelPrefab.InstantiateAsync(transform1.position, transform1.rotation, transform1).Completed +=
                handle => _instance = handle.Result;


            NextLevelNumber = CurrentLevelNumber + 1;
            var nextLevel = itemLevels.Find(level => level.levelNumber == NextLevelNumber);
            NextLevelAvailable = !(nextLevel is null);

            if (nextLevel is null) return;

            UpgradeCost = nextLevel.upgradeCost;
            UpgradeTime = nextLevel.upgradeTime;
            NextLevelRequiredUpgrades = nextLevel.requiredUpgrades;

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
            if (!(_nextLevelPreview is null) && _nextLevelPreview.RuntimeKeyIsValid()) _nextLevelPreview.ReleaseAsset();
        }

        public void FinishUpgrade()
        {
            //TODO: Play some animation, celebration effect etc. before just swapping the current level prefab with next
            UpgradableLevelsData.UpgradablesData[itemKey] = NextLevelNumber;
            UpgradableLevelsData.Save();
            Load(true);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!interactable) return;

            interactionCanvass.SetActive(true);
            interactionCanvass.transform.position = eventData.pointerPressRaycast.worldPosition;
        }

        public void CloseInteractionCanvass()
        {
            interactionCanvass.SetActive(false);
        }

        public void UpgradeCLick()
        {
            UpgradableItemsInteract.instance.Upgrade(this);
        }
    }
}