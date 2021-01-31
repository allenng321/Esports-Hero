using Game.Scripts.GameManagement;
using Game.Scripts.Objects;
using Game.Scripts.Objects.Rooms;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
    public class RoomChangeButton : MonoBehaviour
    {
        [SerializeField] private RequiredUpgrade[] requiredUpgrades;
        [SerializeField] private Button button;
        [SerializeField] private RoomName theRoomName;
        private UpgradableRoom _theRoom;

        private void OnEnable()
        {
            if (!_theRoom) _theRoom = FindObjectOfType<UpgradableRoom>();

            if (theRoomName == _theRoom.RoomName)
            {
                button.interactable = false;
                return;
            }

            var requirementsMet = true;
            foreach (var upgrade in requiredUpgrades)
            {
                if (UpgradableLevelsData.UpgradablesData[upgrade.item] >= upgrade.levelRequired) continue;

                requirementsMet = false;
            }

            button.interactable = requirementsMet;
        }
    }
}