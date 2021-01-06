using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
    public class TabbedPanel : MonoBehaviour
    {
        public Image[] tabButtons;
        public Color inactiveTab, activeTab;

        public GameObject[] tabPanelObjects;

        private void OnEnable()
        {
            SetTab(0);
        }

        public void SetTab(int tabIndex)
        {
            SetTabButtonColor(tabIndex);
            OpenTab(tabIndex);
        }

        private void SetTabButtonColor(int i)
        {
            foreach (var o in tabButtons) o.color = inactiveTab;
            tabButtons[i].color = activeTab;
        }

        private void OpenTab(int i)
        {
            foreach (var o in tabPanelObjects) o.SetActive(false);
            tabPanelObjects[i].SetActive(true);
        }
    }
}