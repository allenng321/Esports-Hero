using UnityEngine;

namespace Game.Scripts.Objects
{
    public class FaceCamera : MonoBehaviour
    {
        private void OnEnable()
        {
            if (!(Camera.main is null)) transform.LookAt(Camera.main.transform);
        }
    }
}