using System;
using UnityEngine;

namespace Game.Scripts.UI
{
    public class MainMenu : MonoBehaviour
    {
        [Serializable]
        public enum CurrentPanel
        {
            Main,
            Settings,
            Credits,
            Info,
            Extras
        }

        [HideInInspector] public CurrentPanel currentPanel = CurrentPanel.Main;

        public GameObject mainPanel, settingsPanel, creditsPanel, infoPanel, extrasPanel;
        public string gameSupportUrl = "http://localhost/esports-test";

        private static LoadingScreen Ls => LoadingScreen.instance;
        public SceneGroup bedroomScene;

        private void Update()
        {
            ManageActivePanels();
        }

        private void ManageActivePanels()
        {
            mainPanel.SetActive(currentPanel == CurrentPanel.Main);

            settingsPanel.SetActive(currentPanel == CurrentPanel.Settings);
            creditsPanel.SetActive(currentPanel == CurrentPanel.Credits);
            infoPanel.SetActive(currentPanel == CurrentPanel.Info);
            extrasPanel.SetActive(currentPanel == CurrentPanel.Extras);
        }

        private void LoadFirstRoom()
        {
            Ls.Load(bedroomScene);
        }

        public void StartNew()
        {
            // TODO: Reset current player data
            LoadFirstRoom();
        }

        public void Continue()
        {
            // TODO: Load player's latest save data into current data
            LoadFirstRoom();
        }

        public void BackToMain()
        {
            currentPanel = CurrentPanel.Main;
        }

        public void OpenExtras()
        {
            currentPanel = CurrentPanel.Extras;
        }

        public void OpenCredits()
        {
            currentPanel = CurrentPanel.Credits;
        }

        public void OpenInfo()
        {
            currentPanel = CurrentPanel.Info;
        }

        public void OpenSettings()
        {
            currentPanel = CurrentPanel.Settings;
        }

        public void OpenSupport()
        {
            Application.OpenURL(gameSupportUrl);
        }
    }
}