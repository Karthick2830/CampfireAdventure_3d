using UnityEngine;

namespace CampfireAdventure.CameraSystem
{
    /// <summary>
    /// Fixed-rotation orthographic "isometric RPG" camera that smoothly follows a target.
    /// The rotation never changes at runtime; only the position follows the target.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class IsometricCameraController : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField, Tooltip("Transform the camera follows (the player).")]
        private Transform target;

        [SerializeField, Tooltip("Offset of the look-at point from the target pivot (e.g. raise to chest height).")]
        private Vector3 targetOffset = new Vector3(0f, 1f, 0f);

        [Header("Framing")]
        [SerializeField, Range(30f, 70f), Tooltip("Downward tilt in degrees. Brief: 45-55.")]
        private float pitch = 50f;

        [SerializeField, Range(0f, 360f), Tooltip("Compass heading of the camera in degrees. 45 = classic isometric.")]
        private float yaw = 45f;

        [SerializeField, Min(5f), Tooltip("Distance from the look-at point along the view direction. Only affects clipping for an orthographic camera.")]
        private float distance = 40f;

        [SerializeField, Min(1f), Tooltip("Orthographic half-height in world units.")]
        private float orthographicSize = 9f;

        [Header("Follow")]
        [SerializeField, Min(0.01f), Tooltip("Approximate time in seconds to catch up with the target.")]
        private float smoothTime = 0.25f;

        [SerializeField, Tooltip("Snap to the target when this component is enabled.")]
        private bool snapOnEnable = true;

        private Camera cam;
        private Vector3 velocity;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        private void Awake()
        {
            cam = GetComponent<Camera>();
            ApplyStaticSettings();
        }

        private void OnEnable()
        {
            if (cam == null) cam = GetComponent<Camera>();
            ApplyStaticSettings();
            if (snapOnEnable) SnapToTarget();
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = ComputeDesiredPosition();
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        }

        /// <summary>Instantly moves the camera to its framed position on the target.</summary>
        public void SnapToTarget()
        {
            if (target == null) return;
            velocity = Vector3.zero;
            transform.position = ComputeDesiredPosition();
        }

        private Quaternion ViewRotation => Quaternion.Euler(pitch, yaw, 0f);

        private Vector3 ComputeDesiredPosition()
        {
            Vector3 lookAt = target.position + targetOffset;
            return lookAt - ViewRotation * Vector3.forward * distance;
        }

        private void ApplyStaticSettings()
        {
            if (cam == null) return;
            cam.orthographic = true;
            cam.orthographicSize = orthographicSize;
            transform.rotation = ViewRotation;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (cam == null) cam = GetComponent<Camera>();
            ApplyStaticSettings();
            if (target != null && !Application.isPlaying) transform.position = ComputeDesiredPosition();
        }
#endif
    }
}
