using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Scripts
{
    [CreateAssetMenu(fileName = "New Scene Group", menuName = "Scriptable Objects/New Scene Group", order = 0)]
    public class SceneGroup : ScriptableObject
    {
        public AssetReferenceSprite loadingImage;
        [SerializeField] public AssetReference[] sceneAssetReferences;
    }
}