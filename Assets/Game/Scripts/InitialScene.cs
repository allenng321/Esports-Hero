using System.Collections;
using Game.Scripts.UI;
using UnityEngine;

namespace Game.Scripts
{
    public class InitialScene : MonoBehaviour
    {
        private static LoadingScreen Ls => LoadingScreen.instance;
        public SceneGroup mainMenuScene;

        private IEnumerator Start()
        {
            while (Ls is null) yield return null;
            Ls.Load(mainMenuScene);
        }
    }
}