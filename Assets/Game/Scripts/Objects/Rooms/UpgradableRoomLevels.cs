using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.Objects.Rooms
{
    [CreateAssetMenu(fileName = "Room Levels", menuName = "Scriptable Objects/New Room Levels Data", order = 0)]
    public class UpgradableRoomLevels : ScriptableObject
    {
        public List<UpgradableObjectLevel> roomLevels;
    }
}