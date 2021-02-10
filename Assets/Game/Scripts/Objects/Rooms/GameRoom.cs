using System.Collections;
using Game.Scripts.GameManagement;
using Game.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Objects.Rooms
{
    public class GameRoom : MonoBehaviour
    {
        [SerializeField] private Text playerNameDisplay, playerExpDisplay;

        [SerializeField] private Text coinsAmountDisplay;

        [SerializeField] private UpgradableRoom theUpgradableRoom;

        [SerializeField] private Text roomLevelDisplay, roomNameDisplay;

        [SerializeField] private GameObject roomChangeCanvass;

        private void Awake() => StartCoroutine(UpdateDisplay());

        private void OnEnable() => StartCoroutine(UpdateDisplay());

        public void UpdateCanvass() => StartCoroutine(UpdateDisplay());

        private IEnumerator UpdateDisplay()
        {
            roomChangeCanvass.SetActive(false);

            if (!theUpgradableRoom) theUpgradableRoom = FindObjectOfType<UpgradableRoom>();

            while (theUpgradableRoom.CurrentLevelNumber == 0)
            {
                yield return null;
            }

            if (!theUpgradableRoom) theUpgradableRoom = GetComponent<UpgradableRoom>();

            playerNameDisplay.text = PlayerSaveData.CurrentData.playerName;
            playerExpDisplay.text = PlayerSaveData.CurrentData.playerExpLevel.ToString();

            coinsAmountDisplay.text = PlayerSaveData.CurrentData.coinsInWallet.ToString();

            roomLevelDisplay.text = theUpgradableRoom.CurrentLevelNumber.ToString();
            roomNameDisplay.text = theUpgradableRoom.ObjectRoom.ToString();
        }

        public void UpgradeRoom()
        {
            UpgradableItemsInteract.instance.Upgrade(theUpgradableRoom);
        }

        public void OpenRoomChangeCanvass()
        {
            roomChangeCanvass.SetActive(true);
        }

        public void ChangeRoom(SceneGroup roomAsset)
        {
            if (roomAsset != theUpgradableRoom.thisRoomScenes) LoadingScreen.instance.Load(roomAsset);
        }
    }
}