using System;
using System.Collections.Generic;
using System.Linq;
using Game.Scripts.GameManagement;
using Game.Scripts.Objects;
using Game.Scripts.Objects.Rooms;
using UnityEngine;
using UnityEngine.AddressableAssets;
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

        public Text currentCoins, moreCoinsNeeded, upgradeCost, upgradeTime;

        private UpgradableRoom _currentRoom;

        public Dictionary<UpgradeData, IUpgradable> CurrentRoomRunningUpgrades { get; } =
            new Dictionary<UpgradeData, IUpgradable>();

        private float _lastUpdate;

        private static int PlayerCurrentCoins => PlayerSaveData.CurrentData.coinsInWallet;

        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeOnLoad()
        {
            if (!(instance is null)) return;

            Addressables.InstantiateAsync(ClassPrefabAddress);
        }

        public void RoomLoaded(RoomName roomName)
        {
            var items = new List<UpgradableItem>(FindObjectsOfType<UpgradableItem>());
            foreach (var ud in UpgradableLevelsData.upgrades[roomName]
                .Where(ud => !CurrentRoomRunningUpgrades.ContainsKey(ud)))
                CurrentRoomRunningUpgrades.Add(ud, items.Find((item => item.itemKey == ud.item)));
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
            var now = DateTime.UtcNow;
            foreach (var key in CurrentRoomRunningUpgrades.Keys.Where(key => now > key.endTime))
            {
                UpgradableLevelsData.FinishRunningUpgrade(key);
                CurrentRoomRunningUpgrades[key].FinishUpgrade();
                CurrentRoomRunningUpgrades.Remove(key);
            }
        }

        public void Upgrade(UpgradableName objectKey, IUpgradable o)
        {
            currentCoins.text = PlayerCurrentCoins.ToString();

            if (PlayerCurrentCoins < o.UpgradeCost)
            {
                coinsLacking.SetActive(true);
                moreCoinsNeeded.text = (o.UpgradeCost - PlayerCurrentCoins).ToString();
            }
            else
            {
                // Just see enough coins or not and not the other checks for now
                upgradeReady.SetActive(true);
                upgradePreview.sprite = o.Preview;
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
                        room = curRoom.roomName,
                        item = objectKey,
                        upgradedVersion = o.NextLevelNumber,
                        startTime = now,
                        endTime = now + o.UpgradeTime
                    };

                    UpgradableLevelsData.StartNewUpgrade(data);
                    CurrentRoomRunningUpgrades.Add(data, o);
                });
            }

            mainPanel.SetActive(true);
        }
    }
}