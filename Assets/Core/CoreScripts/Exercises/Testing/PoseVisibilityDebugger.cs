using UnityEngine;
using ExerciseGame.Core.Pose;

namespace ExerciseGame.Exercises.Debug
{
    public class PoseVisibilityDebugger : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private PoseDataProvider poseProvider;

        [Header("Settings")]
        [SerializeField]
        [Range(0f, 1f)]
        private float visibilityThreshold = 0.75f;

        [Header("Display")]
        [SerializeField]
        private bool showGUI = true;

        [SerializeField]
        private bool showConsoleLog = false;

        [SerializeField]
        private float consoleLogInterval = 1f;

        private float logTimer;


        // =========================================================
        // REQUIRED LANDMARKS
        // =========================================================

        private readonly PoseLandmarkId[] requiredLandmarks =
        {
            PoseLandmarkId.Nose,

            PoseLandmarkId.LeftShoulder,
            PoseLandmarkId.RightShoulder,

            PoseLandmarkId.LeftElbow,
            PoseLandmarkId.RightElbow,

            PoseLandmarkId.LeftWrist,
            PoseLandmarkId.RightWrist,

            PoseLandmarkId.LeftHip,
            PoseLandmarkId.RightHip,

            PoseLandmarkId.LeftKnee,
            PoseLandmarkId.RightKnee,

            PoseLandmarkId.LeftAnkle,
            PoseLandmarkId.RightAnkle
        };


        // =========================================================
        // DISPLAY NAMES
        // =========================================================

        private string GetDisplayName(
            PoseLandmarkId id)
        {
            switch (id)
            {
                case PoseLandmarkId.Nose:
                    return "Nose";

                case PoseLandmarkId.LeftShoulder:
                    return "Left Shoulder";

                case PoseLandmarkId.RightShoulder:
                    return "Right Shoulder";

                case PoseLandmarkId.LeftElbow:
                    return "Left Elbow";

                case PoseLandmarkId.RightElbow:
                    return "Right Elbow";

                case PoseLandmarkId.LeftWrist:
                    return "Left Wrist";

                case PoseLandmarkId.RightWrist:
                    return "Right Wrist";

                case PoseLandmarkId.LeftHip:
                    return "Left Hip";

                case PoseLandmarkId.RightHip:
                    return "Right Hip";

                case PoseLandmarkId.LeftKnee:
                    return "Left Knee";

                case PoseLandmarkId.RightKnee:
                    return "Right Knee";

                case PoseLandmarkId.LeftAnkle:
                    return "Left Ankle";

                case PoseLandmarkId.RightAnkle:
                    return "Right Ankle";

                default:
                    return id.ToString();
            }
        }


        // =========================================================
        // UPDATE
        // =========================================================

        private void Update()
        {
            if (poseProvider == null)
                return;

            if (!showConsoleLog)
                return;

            logTimer += Time.deltaTime;

            if (logTimer < consoleLogInterval)
                return;

            logTimer = 0f;

            LogVisibility();
        }


        // =========================================================
        // CONSOLE DEBUG
        // =========================================================

        private void LogVisibility()
        {
            if (!poseProvider.HasPose)
            {
                UnityEngine.Debug.Log(
                    "[Pose Visibility] NO PERSON");

                return;
            }

            PoseFrame pose =
                poseProvider.CurrentPose;

            string output =
                "[Pose Visibility]\n";

            bool allVisible = true;


            for (int i = 0;
                 i < requiredLandmarks.Length;
                 i++)
            {
                PoseLandmarkId id =
                    requiredLandmarks[i];

                PoseLandmarkData landmark =
                    pose.Get(id);

                float visibility =
                    landmark.visibility;

                bool visible =
                    visibility >=
                    visibilityThreshold;

                if (!visible)
                    allVisible = false;

                output +=
                    $"{GetDisplayName(id)} : " +
                    $"{visibility:F2} " +
                    (visible ? "✓" : "✗") +
                    "\n";
            }


            output +=
                $"ALL VISIBLE: " +
                (allVisible ? "YES" : "NO");


            UnityEngine.Debug.Log(output);
        }


        // =========================================================
        // GUI
        // =========================================================

        private void OnGUI()
        {
            if (!showGUI)
                return;

            if (poseProvider == null)
                return;


            GUIStyle titleStyle =
                new GUIStyle(GUI.skin.label);

            titleStyle.fontSize = 22;
            titleStyle.fontStyle =
                FontStyle.Bold;


            GUIStyle normalStyle =
                new GUIStyle(GUI.skin.label);

            normalStyle.fontSize = 17;


            GUIStyle statusStyle =
                new GUIStyle(GUI.skin.label);

            statusStyle.fontSize = 20;
            statusStyle.fontStyle =
                FontStyle.Bold;


            // =========================================================
            // RIGHT-SIDE PANEL
            // =========================================================

            float panelWidth = 420f;
            float panelHeight = 590f;

            float panelX =
                Screen.width -
                panelWidth -
                20f;

            float panelY = 20f;


            GUI.Box(
                new Rect(
                    panelX,
                    panelY,
                    panelWidth,
                    panelHeight),
                ""
            );


            GUI.Label(
                new Rect(
                    panelX + 20,
                    panelY + 15,
                    panelWidth - 40,
                    35),
                "POSE VISIBILITY DEBUG",
                titleStyle
            );


            // =========================================================
            // NO PERSON
            // =========================================================

            if (!poseProvider.HasPose)
            {
                GUI.Label(
                    new Rect(
                        panelX + 20,
                        panelY + 60,
                        panelWidth - 40,
                        30),
                    "NO PERSON DETECTED",
                    statusStyle
                );

                return;
            }


            PoseFrame pose =
                poseProvider.CurrentPose;


            // =========================================================
            // OVERALL VISIBILITY
            // =========================================================

            float totalVisibility = 0f;

            bool allVisible = true;


            for (int i = 0;
                 i < requiredLandmarks.Length;
                 i++)
            {
                PoseLandmarkData landmark =
                    pose.Get(
                        requiredLandmarks[i]);


                totalVisibility +=
                    Mathf.Clamp01(
                        landmark.visibility);


                if (landmark.visibility <
                    visibilityThreshold)
                {
                    allVisible = false;
                }
            }


            float averageVisibility =
                totalVisibility /
                requiredLandmarks.Length;


            GUI.Label(
                new Rect(
                    panelX + 20,
                    panelY + 60,
                    panelWidth - 40,
                    30),
                $"Average Visibility: " +
                $"{averageVisibility:F2}",
                normalStyle
            );


            GUI.Label(
                new Rect(
                    panelX + 20,
                    panelY + 90,
                    panelWidth - 40,
                    30),
                $"Threshold: " +
                $"{visibilityThreshold:F2}",
                normalStyle
            );


            GUI.Label(
                new Rect(
                    panelX + 20,
                    panelY + 120,
                    panelWidth - 40,
                    30),
                allVisible
                    ? "STATUS: ALL JOINTS VISIBLE ✓"
                    : "STATUS: SOME JOINTS BELOW 75% ✗",
                statusStyle
            );


            // =========================================================
            // JOINT VISIBILITY
            // =========================================================

            float y =
                panelY + 165f;


            for (int i = 0;
                 i < requiredLandmarks.Length;
                 i++)
            {
                PoseLandmarkId id =
                    requiredLandmarks[i];

                PoseLandmarkData landmark =
                    pose.Get(id);


                bool visible =
                    landmark.visibility >=
                    visibilityThreshold;


                GUI.Label(
                    new Rect(
                        panelX + 20,
                        y,
                        260,
                        24),
                    GetDisplayName(id),
                    normalStyle
                );


                GUI.Label(
                    new Rect(
                        panelX + 280,
                        y,
                        110,
                        24),
                    $"{landmark.visibility:F2} " +
                    (visible ? "✓" : "✗"),
                    normalStyle
                );


                y += 28f;
            }
        }
    }
}