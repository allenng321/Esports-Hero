using System;
using Game.Scripts.GameManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
    public class MainMenu : MonoBehaviour
    {
        [Serializable]
        public enum CurrentPanel
        {
            Profile,
            Main,
            Settings,
            Credits,
            Info,
            Extras
        }

        [SerializeField] private CurrentPanel currentPanel = CurrentPanel.Main;

        public GameObject mainPanel, profilePanel, settingsPanel, creditsPanel, infoPanel, extrasPanel;
        public InputField nameInput;
        public Button nameSaveButton;
        public string gameSupportUrl = "http://localhost/esports-test";

        private static LoadingScreen Ls => LoadingScreen.instance;
        public SceneGroup bedroomScene;

        private PlayerData _playerData;

        private void Awake()
        {
            // Call the player current data so trigger the load if not yet loaded
            _playerData = PlayerSaveData.CurrentData;
            if (!PlayerSaveData.SaveExists()) currentPanel = CurrentPanel.Profile;

            nameInput.onValueChanged.AddListener(OnNameChange);
            nameSaveButton.onClick.AddListener(OnSaveName);
        }

        private void Update()
        {
            ManageActivePanels();
        }

        private void ManageActivePanels()
        {
            mainPanel.SetActive(currentPanel == CurrentPanel.Main);
            profilePanel.SetActive(currentPanel == CurrentPanel.Profile);
            settingsPanel.SetActive(currentPanel == CurrentPanel.Settings);
            creditsPanel.SetActive(currentPanel == CurrentPanel.Credits);
            infoPanel.SetActive(currentPanel == CurrentPanel.Info);
            extrasPanel.SetActive(currentPanel == CurrentPanel.Extras);
        }

        private void LoadFirstRoom()
        {
            Ls.Load(bedroomScene);
        }

        /*public void StartNew()
        {
            var d = PlayerSaveData.defaultData;
            d.playerName = _playerData.playerName;
            PlayerSaveData.CurrentData = d;
            LoadFirstRoom();
        }

        public void Continue()
        {
            PlayerSaveData.Load();
            LoadFirstRoom();
        }*/

        public void Play()
        {
            PlayerSaveData.Load();
            LoadFirstRoom();
        }

        public void BackToMain()
        {
            currentPanel = CurrentPanel.Main;
        }

        private void OnSaveName()
        {
            var d = PlayerSaveData.defaultData;
            d.playerName = nameInput.text;
            PlayerSaveData.CurrentData = d;
            PlayerSaveData.Save();
        }

        private void OnNameChange(string newName)
        {
            nameSaveButton.interactable = newName.Length >= 4;
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