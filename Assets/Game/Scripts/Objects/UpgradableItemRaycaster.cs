using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Scripts.Objects
{
    /// <summary>
    /// Simple event system using physics raycasts.
    /// Raycaster for casting against 3D Physics components of the UpgradableItem(s) present in the scene.
    /// </summary>
    [AddComponentMenu("Event/Custom/UpgradableItemRaycaster")]
    [RequireComponent(typeof(Camera))]
    public sealed class UpgradableItemRaycaster : PhysicsRaycaster
    {
        private UpgradableItemRaycaster()
        {
        }

        // Override the base Raycast method to only do raycast for 1 hit.
        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            Ray ray = new Ray();
            int displayIndex = 0;
            float distanceToClipPlane = 0;
            if (!ComputeRayAndDistance(eventData, ref ray, ref displayIndex, ref distanceToClipPlane))
                return;

            if (!Physics.Raycast(ray, out var hit, distanceToClipPlane, finalEventMask)) return;

            var result = new RaycastResult
            {
                gameObject = hit.collider.gameObject,
                module = this,
                distance = hit.distance,
                worldPosition = hit.point,
                worldNormal = hit.normal,
                screenPosition = eventData.position,
                displayIndex = displayIndex,
                index = resultAppendList.Count,
                sortingLayer = 0,
                sortingOrder = 0
            };
            resultAppendList.Add(result);
        }
    }
}
