using Mediapipe.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CampfireAdventure.UI
{
    /// <summary>
    /// Shows MediaPipe's tracking canvas (webcam image + skeleton, built from the package's Main Canvas and
    /// Annotatable Screen prefabs) as a small picture in the HUD. The canvas is drawn by its own camera, parked
    /// far from the play area, into a RenderTexture that a RawImage displays, so it never covers the game.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TrackingPreview : MonoBehaviour
    {
        [SerializeField] private Canvas trackingCanvas;
        [SerializeField, Tooltip("The HUD picture that shows the preview.")] private RawImage target;
        [SerializeField] private MultiPoseLandmarkListWithMaskAnnotation skeleton;

        [Header("Preview")]
        [SerializeField] private Vector2Int textureSize = new Vector2Int(640, 360);
        [SerializeField, Min(0.01f)] private float skeletonLandmarkRadius = 0.4f;
        [SerializeField, Min(0.01f)] private float skeletonConnectionWidth = 0.1f;
        [SerializeField, Tooltip("Where the preview camera sits; far below the level so the game camera never sees it.")]
        private Vector3 cameraPosition = new Vector3(0f, -5000f, 0f);

        private RenderTexture texture;
        private Camera previewCamera;

        private void Awake()
        {
            // Only the camera picture is wanted: hide the sample's header, footer and labels.
            foreach (string part in new[] { "Header", "Footer" })
            {
                var t = trackingCanvas.transform.Find("Container Panel/" + part);
                if (t != null) t.gameObject.SetActive(false);
            }
            foreach (var label in trackingCanvas.GetComponentsInChildren<TMP_Text>(true))
                if (label.transform.parent != null && label.transform.parent.name == "Body") label.gameObject.SetActive(false);

            if (skeleton != null)
            {
                skeleton.SetLandmarkRadius(skeletonLandmarkRadius);
                skeleton.SetConnectionWidth(skeletonConnectionWidth);
            }

            texture = new RenderTexture(textureSize.x, textureSize.y, 16) { name = "TrackingPreview" };

            var cameraObject = new GameObject("TrackingPreviewCamera");
            // Not a child of the canvas: a Screen Space - Camera canvas moves itself in front of its camera,
            // so a camera parented to it would drift away with it.
            cameraObject.transform.position = cameraPosition;
            var cam = cameraObject.AddComponent<Camera>();
            previewCamera = cam;
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 50f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.depth = -10f;
            cam.targetTexture = texture;

            trackingCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            trackingCanvas.worldCamera = cam;
            trackingCanvas.planeDistance = 10f;

            if (target != null) target.texture = texture;
        }

        private void OnDestroy()
        {
            if (previewCamera != null) Destroy(previewCamera.gameObject);
            if (texture == null) return;
            texture.Release();
            Destroy(texture);
        }
    }
}
