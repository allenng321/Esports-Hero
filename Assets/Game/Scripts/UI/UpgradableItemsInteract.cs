using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Game.Scripts.GameManagement;
using Game.Scripts.Objects;
using Game.Scripts.Objects.Rooms;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
    public class UpgradableItemsInteract : MonoBehaviour
    {
        public static UpgradableItemsInteract instance;
        private const string ClassPrefabAddress = "UpgradableItemsInteractionPrefab";

        private Sprite _defaultPreviewSprite;

        public GameObject mainPanel, upgradeReady, upgradeRunning, coinsLacking, maxLevelReached, otherUpgradeRequired;
        public Button confirmUpgrade;

        public Image upgradePreview;

        public Text selectedItem,
            currentLevel,
            nextLevel,
            upgradedLevel,
            currentCoins,
            moreCoinsNeeded,
            upgradeCost,
            upgradeTime,
            upgradeFinishingOn;

        public Transform requiredUpgradesListParent;
        public Text requiredUpgradesListItem;

        public Dictionary<UpgradeData, IUpgradable> CurrentRoomRunningUpgrades { get; } =
            new Dictionary<UpgradeData, IUpgradable>();

        private float _lastUpdate;

        private static int PlayerCurrentCoins => PlayerSaveData.CurrentData.coinsInWallet;

        private readonly UnityEvent _runningUpgradesBuffer = new UnityEvent();

        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeOnLoad()
        {
            if (!(instance is null)) return;

            Addressables.InstantiateAsync(ClassPrefabAddress);
        }

        public void RoomLoaded(RoomName roomName, List<UpgradableItem> items)
        {
            foreach (var ud in UpgradableLevelsData.RunningUpgrades[roomName]
                .Where(ud => !CurrentRoomRunningUpgrades.ContainsKey(ud)))
                CurrentRoomRunningUpgrades.Add(ud, items.Find(item => item.ObjectKey == ud.item));

            if (mainPanel.activeSelf) ClosePanels();
        }

        public void StopUpgrade(UpgradeData data)
        {
            if (mainPanel.activeSelf) ClosePanels();
            if (CurrentRoomRunningUpgrades.ContainsKey(data)) CurrentRoomRunningUpgrades.Remove(data);
        }

        public void ClearCurrentRoomUpgrades(RoomName roomName)
        {
            foreach (var key in CurrentRoomRunningUpgrades.Keys.Where(key => key.room == roomName))
                CurrentRoomRunningUpgrades.Remove(key);
        }

        public void ClosePanels()
        {
            mainPanel.SetActive(false);
            upgradeReady.SetActive(false);
            upgradeRunning.SetActive(false);
            coinsLacking.SetActive(false);
            maxLevelReached.SetActive(false);
            otherUpgradeRequired.SetActive(false);

            upgradePreview.sprite = _defaultPreviewSprite;
        }

        private void OnEnable()
        {
            if (!(instance is null)) Destroy(gameObject);
            else
            {
                instance = this;
                DontDestroyOnLoad(instance);

                _defaultPreviewSprite = upgradePreview.sprite;

                ClosePanels();
            }
        }

        private void Update()
        {
            var now = Time.time;

            // Update the below code only once a second
            if (now - _lastUpdate < 1) return;

            CurrentRoomUpgradesCheck();

            _lastUpdate = now;
        }

        private void CurrentRoomUpgradesCheck()
        {
            var dataDirty = false;
            var now = DateTime.UtcNow;
            foreach (var key in CurrentRoomRunningUpgrades.Keys.Where(key => now > key.endTime))
            {
                dataDirty = true;
                CurrentRoomRunningUpgrades[key].FinishUpgrade();
                UpgradableLevelsData.FinishRunningUpgrade(key);

                _runningUpgradesBuffer.AddListener(() => StopUpgrade(key));
                // CurrentRoomRunningUpgrades.Remove(key);
            }

            if (!dataDirty) return;
            UpgradableLevelsData.Save();
            _runningUpgradesBuffer.Invoke();
            _runningUpgradesBuffer.RemoveAllListeners();
        }

        public void Upgrade(IUpgradable o)
        {
            var keyName = o.ObjectKey.ToString();
            selectedItem.text = keyName.Substring(0, keyName.Length - 5);
            currentLevel.text = o.CurrentLevelNumber.ToString();
            nextLevel.text = o.NextLevelNumber.ToString();
            currentCoins.text = PlayerCurrentCoins.ToString();

            var requirementsMet = true;
            var requiredUpgrades = new List<RequiredUpgrade>();

            upgradePreview.gameObject.SetActive(true);
            if (o.Preview) upgradePreview.sprite = o.Preview;

            if (o.NextLevelRequiredUpgrades != null)
                foreach (var upgrade in o.NextLevelRequiredUpgrades)
                {
                    if (UpgradableLevelsData.UpgradablesData[upgrade.item] >= upgrade.levelRequired) continue;

                    requirementsMet = false;
                    requiredUpgrades.Add(upgrade);
                }

            if (UpgradableLevelsData.TryGetRunningUpgrade(o.ObjectKey, out var ud))
            {
                upgradeRunning.SetActive(true);
                upgradeFinishingOn.text =
                    ((DateTime) ud.endTime).ToLocalTime().ToString(DateTimeFormatInfo.CurrentInfo);
                upgradedLevel.text = ud.upgradedVersion.ToString();
            }
            else if (!o.NextLevelAvailable)
            {
                maxLevelReached.SetActive(true);
                upgradePreview.gameObject.SetActive(false);
            }
            else if (!requirementsMet)
            {
                otherUpgradeRequired.SetActive(true);
                upgradePreview.gameObject.SetActive(false);

                foreach (var upgrade in requiredUpgrades)
                {
                    var t = Instantiate(requiredUpgradesListItem, requiredUpgradesListParent.position,
                        requiredUpgradesListParent.rotation, requiredUpgradesListParent);
                    t.text = $"{upgrade.item.ToString()}:{upgrade.levelRequired.ToString()} required";
                }
            }
            else if (PlayerCurrentCoins < o.UpgradeCost)
            {
                coinsLacking.SetActive(true);
                moreCoinsNeeded.text = (o.UpgradeCost - PlayerCurrentCoins).ToString();
            }
            else
            {
                // After those two checks the upgradable should be ready now,
                // We will do the required upgrades later
                upgradeReady.SetActive(true);
                upgradeCost.text = o.UpgradeCost.ToString();
                upgradeTime.text = o.UpgradeTime.ToString();

                confirmUpgrade.onClick.RemoveAllListeners();
                confirmUpgrade.onClick.AddListener(delegate
                {
                    ClosePanels();

                    var playerData = PlayerSaveData.CurrentData;
                    playerData.coinsInWallet = Mathf.Clamp(playerData.coinsInWallet - o.UpgradeCost, 0, int.MaxValue);
                    PlayerSaveData.CurrentData = playerData;
                    PlayerSaveData.Save();

                    var now = DateTime.UtcNow;
                    var curRoom = FindObjectOfType<UpgradableRoom>();

                    if (((TimeSpan) o.UpgradeTime).TotalSeconds < 1)
                    {
                        o.FinishUpgrade();
                        return;
                    }

                    var data = new UpgradeData
                    {
                        room = curRoom.ObjectRoom,
                        item = o.ObjectKey,
                        upgradedVersion = o.NextLevelNumber,
                        startTime = now,
                        endTime = now + o.UpgradeTime
                    };

                    UpgradableLevelsData.StartNewUpgrade(data);
                    UpgradableLevelsData.Save();
                    CurrentRoomRunningUpgrades.Add(data, o);
                });
            }

            mainPanel.SetActive(true);
        }
    }
}