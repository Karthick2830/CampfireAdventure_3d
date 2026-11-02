using ExerciseGame.Core.Pose;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CampfireAdventure.UI
{
    /// <summary>
    /// Header of the camera card: a status dot + text for the tracking quality, and a LIVE badge that is
    /// shown while pose results keep arriving. Read-only; it never changes tracking.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PoseStatusView : MonoBehaviour
    {
        [SerializeField] private PoseDataProvider pose;
        [SerializeField] private Image dot;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private GameObject liveBadge;

        [SerializeField, Min(0.1f), Tooltip("No new result for this long hides the LIVE badge.")] private float liveTimeout = 1.5f;

        private static readonly Color Good = new Color(0.35f, 0.85f, 0.35f);
        private static readonly Color Weak = new Color(1f, 0.75f, 0.2f);
        private static readonly Color None = new Color(0.9f, 0.3f, 0.25f);

        // The exercise only needs the arms, so "Tracking" means both shoulders, elbows and wrists are seen.
        private static readonly PoseLandmarkId[] ArmLandmarks =
        {
            PoseLandmarkId.LeftShoulder, PoseLandmarkId.RightShoulder,
            PoseLandmarkId.LeftElbow, PoseLandmarkId.RightElbow,
            PoseLandmarkId.LeftWrist, PoseLandmarkId.RightWrist,
        };

        private int lastFrame = -1;
        private float lastChange = -999f;

        private void Update()
        {
            if (pose == null) return;

            if (pose.FrameCount != lastFrame) { lastFrame = pose.FrameCount; lastChange = Time.unscaledTime; }
            bool live = Time.unscaledTime - lastChange < liveTimeout;
            if (liveBadge != null && liveBadge.activeSelf != live) liveBadge.SetActive(live);

            Color color; string text;
            if (!live) { color = None; text = "No camera"; }
            else if (!pose.HasPose) { color = None; text = "No person"; }
            else if (pose.HasRequiredLandmarks(ArmLandmarks)) { color = Good; text = "Tracking"; }
            else { color = Weak; text = "Show both arms"; }

            if (dot != null) dot.color = color;
            if (statusText != null) statusText.text = text;
        }
    }
}
