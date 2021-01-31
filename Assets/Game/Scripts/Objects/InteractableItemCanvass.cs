using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Objects
{
    public class InteractableItemCanvass : MonoBehaviour
    {
        private static InteractableItemCanvass _currentActive;

        [Serializable]
        public class LevelLockedButton
        {
            public Button button;
            public int itemLevelRequired;
        }

        [SerializeField] protected LevelLockedButton[] lockedButtons;
        [SerializeField] protected UpgradableItem item;

        protected virtual void OnEnable()
        {
            if (_currentActive && _currentActive != this) _currentActive.ExitCanvass();
            _currentActive = this;

            if (!(Camera.main is null))
                transform.forward = (transform.position - Camera.main.transform.position).normalized;

            item = item ? item : GetComponentInParent<UpgradableItem>();
            foreach (var lb in lockedButtons)
            {
                lb.button.interactable = item.CurrentLevelNumber >= lb.itemLevelRequired;
            }
        }

        public virtual void ExitCanvass()
        {
            item.CloseInteractionCanvass();
        }

        public void Upgrade()
        {
            ExitCanvass();
            item.UpgradeCLick();
        }
    }
}