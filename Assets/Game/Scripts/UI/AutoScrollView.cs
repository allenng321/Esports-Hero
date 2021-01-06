using UnityEngine;

namespace Game.Scripts.UI
{
    public class AutoScrollView : MonoBehaviour
    {
        public RectTransform contentTransform;
        public float scrollPerSecond;

        void Update()
        {
            var p = contentTransform.position;
            p = new Vector3(p.x, Mathf.Clamp(p.y + scrollPerSecond * Time.deltaTime, 0, contentTransform.sizeDelta.y),
                p.z);
            contentTransform.position = p;
        }
    }
}