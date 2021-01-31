using UnityEngine;

namespace Game.Scripts.Objects
{
    public class LookAtCamera : MonoBehaviour
    {
        protected virtual void OnEnable()
        {
            if (!(Camera.main is null)) transform.LookAt(Camera.main.transform);
        }
    }
}