using System.Collections;
using Game.Scripts.GameManagement;
using Game.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Objects.Rooms
{
    public class GameRoom : MonoBehaviour
    {
        public Text playerNameDisplay, playerExpDisplay;

        public UpgradableRoom theUpgradableRoom;

        public Text roomLevelDisplay, roomNameDisplay;
        public Button roomLevelButton;

        private void Awake() => StartCoroutine(UpdateDisplay());

        private void OnEnable() => StartCoroutine(UpdateDisplay());

        private IEnumerator UpdateDisplay()
        {
            while (theUpgradableRoom.CurrentLevelNumber == 0)
            {
                yield return null;
            }

            if (!theUpgradableRoom) theUpgradableRoom = GetComponent<UpgradableRoom>();

            playerNameDisplay.text = PlayerSaveData.CurrentData.playerName;
            playerExpDisplay.text = PlayerSaveData.CurrentData.playerExp.ToString();

            roomLevelDisplay.text = theUpgradableRoom.CurrentLevelNumber.ToString();
            roomNameDisplay.text = theUpgradableRoom.ObjectRoom.ToString();

            roomLevelButton.onClick.RemoveAllListeners();
            roomLevelButton.onClick.AddListener(UpgradeRoom);
        }

        private void UpgradeRoom()
        {
            UpgradableItemsInteract.instance.Upgrade(theUpgradableRoom);
        }
    }
}