using Game.Scripts.GameManagement;
using Game.Scripts.Objects;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
    public class RoomChangeButton : MonoBehaviour
    {
        [SerializeField] private RequiredUpgrade[] requiredUpgrades;
        [SerializeField] private Button button;

        private void OnEnable()
        {
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