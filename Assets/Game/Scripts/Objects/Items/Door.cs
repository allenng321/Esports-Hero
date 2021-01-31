using Game.Scripts.GameManagement;
using Game.Scripts.Objects.Rooms;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Scripts.Objects.Items
{
    public class Door : MonoBehaviour, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            // Can't leave room when gaming
            if (PlayerManager.isUsingPC) return;

            FindObjectOfType<GameRoom>().OpenRoomChangeCanvass();
        }
    }
}