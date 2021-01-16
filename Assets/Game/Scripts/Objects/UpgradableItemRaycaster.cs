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


/*

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Scripts.Objects
{
    /// <summary>
    /// Simple event system using physics raycasts.
    /// </summary>
    /// /// <summary>
    /// Simple event system using physics raycasts.
    /// Raycaster for casting against 3D Physics components of the UpgradableItem(s) present in the scene.
    /// </summary>
    [AddComponentMenu("Event/Custom/UpgradableItemRaycaster")]
    [RequireComponent(typeof(Camera))]
    public class UpgradableItemRaycaster : BaseRaycaster
    {
        /// <summary>
        /// Const to use for clarity when no event mask is set
        /// </summary>
        protected const int KNoEventMaskSet = -1;

        protected Camera mEventCamera;

        /// <summary>
        /// Layer mask used to filter events. Always combined with the camera's culling mask if a camera is used.
        /// </summary>
        [SerializeField] protected LayerMask mEventMask = KNoEventMaskSet;

        protected int mLastMaxRayIntersections = 0;

        protected UpgradableItemRaycaster()
        {
        }

        public override Camera eventCamera
        {
            get
            {
                if (mEventCamera == null)
                    mEventCamera = GetComponent<Camera>();
                return mEventCamera ? mEventCamera : Camera.main;
            }
        }


        /// <summary>
        /// Depth used to determine the order of event processing.
        /// </summary>
        public virtual int Depth => (eventCamera != null) ? (int) eventCamera.depth : 0xFFFFFF;

        /// <summary>
        /// Event mask used to determine which objects will receive events.
        /// </summary>
        public int FinalEventMask => (eventCamera != null) ? eventCamera.cullingMask & mEventMask : KNoEventMaskSet;

        /// <summary>
        /// Layer mask used to filter events. Always combined with the camera's culling mask if a camera is used.
        /// </summary>
        public LayerMask EventMask
        {
            get { return mEventMask; }
            set { mEventMask = value; }
        }

        /// <summary>
        /// Returns a ray going from camera through the event position and the distance between the near and far clipping planes along that ray.
        /// </summary>
        /// <param name="eventData">The pointer event for which we will cast a ray.</param>
        /// <param name="ray">The ray to use.</param>
        /// <param name="eventDisplayIndex">The display index used.</param>
        /// <param name="distanceToClipPlane">The distance between the near and far clipping planes along the ray.</param>
        /// <returns>True if the operation was successful. false if it was not possible to compute, such as the eventPosition being outside of the view.</returns>
        protected bool ComputeRayAndDistance(PointerEventData eventData, ref Ray ray, ref int eventDisplayIndex,
            ref float distanceToClipPlane)
        {
            if (eventCamera == null)
                return false;

            var eventPosition = Display.RelativeMouseAt(eventData.position);
            if (eventPosition != Vector3.zero)
            {
                // We support multiple display and display identification based on event position.
                eventDisplayIndex = (int) eventPosition.z;

                // Discard events that are not part of this display so the user does not interact with multiple displays at once.
                if (eventDisplayIndex != eventCamera.targetDisplay)
                    return false;
            }
            else
            {
                // The multiple display system is not supported on all platforms, when it is not supported the returned position
                // will be all zeros so when the returned index is 0 we will default to the event data to be safe.
                eventPosition = eventData.position;
            }

            // Cull ray casts that are outside of the view rect. (case 636595)
            if (!eventCamera.pixelRect.Contains(eventPosition))
                return false;

            ray = eventCamera.ScreenPointToRay(eventPosition);
            // compensate far plane distance - see MouseEvents.cs
            float projectionDirection = ray.direction.z;
            distanceToClipPlane = Mathf.Approximately(0.0f, projectionDirection)
                ? Mathf.Infinity
                : Mathf.Abs((eventCamera.farClipPlane - eventCamera.nearClipPlane) / projectionDirection);
            return true;
        }

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            Ray ray = new Ray();
            int displayIndex = 0;
            float distanceToClipPlane = 0;
            if (!ComputeRayAndDistance(eventData, ref ray, ref displayIndex, ref distanceToClipPlane))
                return;

            if (Physics.Raycast(ray, out var hit, distanceToClipPlane, FinalEventMask))
            {
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
}

*/