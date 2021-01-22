using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Objects
{
    public abstract class InteractableItemCanvass : MonoBehaviour
    {
        [Serializable]
        public class LevelLockedButton
        {
            public Button button;
            public int itemLevelRequired;
        }

        [SerializeField] protected LevelLockedButton[] lockedButtons;
        [SerializeField] protected UpgradableItem item;

        private void OnEnable()
        {
            foreach (var lb in lockedButtons)
            {
                lb.button.interactable = item.CurrentLevelNumber >= lb.itemLevelRequired;
            }
        }
    }
}