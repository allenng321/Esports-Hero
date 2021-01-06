using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
    public class LoadingScreen : MonoBehaviour
    {
        public static LoadingScreen Instance;
        private const string LoadingScreenPrefabText = "LoadingScreenPrefab";

        private Sprite _defaultLoadingBackground;

        private bool _loading;
        private AsyncOperationHandle<Sprite> _loadingSprite;

        public GameObject mainPanel;

        public Image loadingBackground;

        public Slider progressBar;
        public Text progressPercent;

        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeOnLoad()
        {
            if (!(Instance is null)) return;

            Addressables.InstantiateAsync(LoadingScreenPrefabText);
        }

        private void OnEnable()
        {
            if (!(Instance is null)) Destroy(gameObject);
            else
            {
                Instance = this;
                DontDestroyOnLoad(Instance);
                mainPanel.SetActive(false);

                _defaultLoadingBackground = loadingBackground.sprite;
            }
        }

        public void Load(SceneGroup asset)
        {
            if (_loading)
                throw new ApplicationException("Already busy loading another scene.");

            _loading = true;

            mainPanel.SetActive(true);
            _loadingSprite = asset.loadingImage.LoadAssetAsync();
            _loadingSprite.Completed += delegate(AsyncOperationHandle<Sprite> handle)
            {
                loadingBackground.sprite = handle.Result;
            };

            progressBar.value = progressBar.minValue = 0;
            progressBar.maxValue = asset.sceneAssetReferences.Length;

            StartCoroutine(LoadScenes(asset.sceneAssetReferences));
        }

        private IEnumerator LoadScenes(IReadOnlyList<AssetReference> scenes)
        {
            var firstScene = scenes[0];
            var loadingOperation = firstScene.LoadSceneAsync();
            float mv = progressBar.maxValue, v;

            while (!loadingOperation.IsDone)
            {
                v = progressBar.value = Mathf.Clamp01(loadingOperation.PercentComplete);
                progressPercent.text = Mathf.Round(v * 100 / mv) + "%";
                yield return null;
            }

            for (var i = 1; i < scenes.Count; i++)
            {
                var scene = scenes[i];
                loadingOperation = scene.LoadSceneAsync(LoadSceneMode.Additive);
                while (!loadingOperation.IsDone)
                {
                    v = progressBar.value = i + Mathf.Clamp01(loadingOperation.PercentComplete);
                    progressPercent.text = Mathf.Round(v * 100 / mv) + "%";
                    yield return null;
                }
            }

            progressBar.value = mv;
            progressPercent.text = "100%";

            mainPanel.SetActive(false);

            loadingBackground.sprite = _defaultLoadingBackground;
            Addressables.Release(_loadingSprite);
            _loading = false;
        }
    }
}