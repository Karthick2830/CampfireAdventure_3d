using UnityEngine;
using ExerciseGame.Core.Exercise;
using ExerciseGame.Core.Pose;
using ExerciseGame.Core.UI;
using ExerciseGame.Exercises.StandingLateralObliqueStretch;

namespace ExerciseGame.Exercises.StandingLateralObliqueStretch.Debug
{
    public class StandingLateralObliqueValidatorTester :
    MonoBehaviour,
    IExerciseValidator,
    IExerciseValidatorFeedback
    {
        // =========================================================
        // REFERENCES
        // =========================================================

        [Header("References")]

        [SerializeField]
        private PoseDataProvider poseProvider;

        [SerializeField]
        private StandingLateralObliqueValidator validator;


        // =========================================================
        // DEBUG
        // =========================================================

        [Header("Debug")]

        [SerializeField]
        private bool showDebugGUI = true;

        [SerializeField]
        private bool logToConsole = true;

        [SerializeField]
        private float logInterval = 0.25f;


        // =========================================================
        // RUNTIME
        // =========================================================

        private ValidationResult lastResult;

        private float logTimer;

        private bool validationPaused;


        // =========================================================
        // PUBLIC API
        // =========================================================

        public ValidationResult LastResult
        {
            get
            {
                return lastResult;
            }
        }


        public StandingLateralObliqueValidator Validator
        {
            get
            {
                return validator;
            }
        }


        // =========================================================
        // IExerciseValidator
        // =========================================================

        public ExerciseState CurrentState
        {
            get
            {
                if (validator == null)
                    return default;

                return validator.CurrentState;
            }
        }


        public bool IsRepCompleted
        {
            get
            {
                return validator != null &&
                       validator.IsRepCompleted;
            }
        }


        /// <summary>
        /// Required by IExerciseValidator.
        /// The tester forwards the pose to the actual validator.
        /// </summary>
        public ValidationResult Validate(
            PoseFrame pose,
            float deltaTime)
        {
            if (validator == null)
            {
                return default;
            }

            lastResult =
                validator.Validate(
                    pose,
                    deltaTime);

            return lastResult;
        }


        /// <summary>
        /// Required by IExerciseValidator.
        /// </summary>
        public void ConsumeRep()
        {
            if (validator == null)
                return;

            validator.ConsumeRep();
        }


        /// <summary>
        /// Required by IExerciseValidator.
        /// </summary>
        public void Reset()
        {
            ResetValidation();
        }


        // =========================================================
        // UNITY
        // =========================================================

        private void Awake()
        {
            if (poseProvider == null)
            {
                poseProvider =
                    FindFirstObjectByType<PoseDataProvider>();
            }

            if (validator == null)
            {
                validator =
                    FindFirstObjectByType<
                        StandingLateralObliqueValidator>();
            }
        }


        private void Update()
        {
            if (validationPaused)
                return;

            if (poseProvider == null ||
                validator == null)
            {
                return;
            }


            // =====================================================
            // GET POSE
            // =====================================================

            PoseFrame pose =
                poseProvider.CurrentPose;


            // =====================================================
            // VALIDATION
            // =====================================================

            Validate(
                pose,
                Time.deltaTime);


            // =====================================================
            // DEBUG LOG
            // =====================================================

            logTimer +=
                Time.deltaTime;


            if (logToConsole &&
                logTimer >= logInterval)
            {
                logTimer = 0f;

                UnityEngine.Debug.Log(
                    "[Standing Lateral Oblique] " +

                    "State: " +
                    validator.CurrentState +

                    " | Torso: " +
                    validator.CurrentTorsoAngle
                        .ToString("F1") +
                    "°" +

                    " | Right Arm: " +
                    validator.CurrentRightArmOverheadAngle
                        .ToString("F1") +
                    "°" +

                    " | Right Overhead: " +
                    validator.IsRightArmOverhead +

                    " | Left Arm: " +
                    validator.CurrentLeftArmOverheadAngle
                        .ToString("F1") +
                    "°" +

                    " | Left Overhead: " +
                    validator.IsLeftArmOverhead +

                    " | Active Arm: " +
                    validator.ActiveArm +

                    " | Expected Bend: " +
                    validator.ExpectedBendDirection +

                    " | Quality: " +
                    lastResult.quality
                        .ToString("F2") +

                    " | Confidence: " +
                    lastResult.confidence
                        .ToString("F2") +

                    " | Valid: " +
                    lastResult.isValid +

                    " | Rep Completed: " +
                    IsRepCompleted +

                    " | Feedback: " +
                    lastResult.feedback);
            }


            // =====================================================
            // IMPORTANT
            // =====================================================
            //
            // DO NOT COUNT REPS HERE.
            //
            // ExerciseManager owns:
            //
            // CurrentRep
            // CurrentSet
            // Rep completion
            // Set completion
            // Exercise completion
            //
        }


        // =========================================================
        // RESET VALIDATION
        // =========================================================

        public void ResetValidation()
        {
            lastResult =
                default;

            logTimer = 0f;

            validationPaused = false;


            if (validator != null)
            {
                validator.Reset();
            }
        }


        // =========================================================
        // PAUSE
        // =========================================================

        public void PauseValidation()
        {
            validationPaused = true;
        }


        // =========================================================
        // RESUME
        // =========================================================

        public void ResumeValidation()
        {
            validationPaused = false;
        }


        // =========================================================
        // GUI
        // =========================================================

        private void OnGUI()
        {
            if (!showDebugGUI ||
                validator == null ||
                poseProvider == null)
            {
                return;
            }


            float panelWidth = 560f;

            float panelHeight = 600f;

            float panelX =
                Screen.width -
                panelWidth -
                25f;

            float panelY = 25f;


            // =====================================================
            // STYLES
            // =====================================================

            GUIStyle titleStyle =
                new GUIStyle(
                    GUI.skin.label);

            titleStyle.fontSize = 26;

            titleStyle.fontStyle =
                FontStyle.Bold;


            GUIStyle normalStyle =
                new GUIStyle(
                    GUI.skin.label);

            normalStyle.fontSize = 20;


            GUIStyle valueStyle =
                new GUIStyle(
                    GUI.skin.label);

            valueStyle.fontSize = 24;

            valueStyle.fontStyle =
                FontStyle.Bold;


            GUIStyle feedbackStyle =
                new GUIStyle(
                    GUI.skin.label);

            feedbackStyle.fontSize = 22;

            feedbackStyle.wordWrap = true;


            // =====================================================
            // BOX
            // =====================================================

            GUI.Box(
                new Rect(
                    panelX,
                    panelY,
                    panelWidth,
                    panelHeight),
                "");


            float x =
                panelX + 25f;


            // =====================================================
            // TITLE
            // =====================================================

            GUI.Label(
                new Rect(
                    x,
                    panelY + 20f,
                    500f,
                    40f),
                "STANDING LATERAL OBLIQUE",
                titleStyle);


            // =====================================================
            // TRACKING
            // =====================================================

            GUI.Label(
                new Rect(
                    x,
                    panelY + 70f,
                    500f,
                    30f),
                "Tracking: " +
                poseProvider.CurrentTrackingQuality,
                normalStyle);


            // =====================================================
            // STATE
            // =====================================================

            GUI.Label(
                new Rect(
                    x,
                    panelY + 105f,
                    500f,
                    30f),
                "State: " +
                validator.CurrentState,
                valueStyle);


            // =====================================================
            // POSE READY
            // =====================================================

            GUI.Label(
                new Rect(
                    x,
                    panelY + 140f,
                    500f,
                    30f),
                "Pose Ready: " +
                validator.IsPoseReady,
                normalStyle);


            // =====================================================
            // NEXT ARM
            // =====================================================

            GUI.Label(
                new Rect(
                    x,
                    panelY + 175f,
                    500f,
                    30f),
                "NEXT ARM: " +
                validator.NextArm,
                valueStyle);


            // =====================================================
            // ACTIVE ARM
            // =====================================================

            GUI.Label(
                new Rect(
                    x,
                    panelY + 210f,
                    500f,
                    30f),
                "ACTIVE ARM: " +
                validator.ActiveArm,
                valueStyle);


            // =====================================================
            // EXPECTED BEND
            // =====================================================

            GUI.Label(
                new Rect(
                    x,
                    panelY + 245f,
                    500f,
                    30f),
                "Expected Bend: " +
                validator.ExpectedBendDirection,
                valueStyle);


            // =====================================================
            // TORSO
            // =====================================================

            GUI.Label(
                new Rect(
                    x,
                    panelY + 285f,
                    500f,
                    30f),
                "Torso Angle: " +
                validator.CurrentTorsoAngle
                    .ToString("F1") +
                "°",
                valueStyle);


            // =====================================================
            // RIGHT ARM
            // =====================================================

            GUI.Label(
                new Rect(
                    x,
                    panelY + 325f,
                    500f,
                    30f),
                "Right Arm Overhead: " +
                validator.CurrentRightArmOverheadAngle
                    .ToString("F1") +
                "°",
                valueStyle);


            GUI.Label(
                new Rect(
                    x,
                    panelY + 355f,
                    500f,
                    30f),
                "Right Overhead: " +
                validator.IsRightArmOverhead,
                normalStyle);


            // =====================================================
            // LEFT ARM
            // =====================================================

            GUI.Label(
                new Rect(
                    x,
                    panelY + 390f,
                    500f,
                    30f),
                "Left Arm Overhead: " +
                validator.CurrentLeftArmOverheadAngle
                    .ToString("F1") +
                "°",
                valueStyle);


            GUI.Label(
                new Rect(
                    x,
                    panelY + 420f,
                    500f,
                    30f),
                "Left Overhead: " +
                validator.IsLeftArmOverhead,
                normalStyle);


            // =====================================================
            // QUALITY
            // =====================================================

            GUI.Label(
                new Rect(
                    x,
                    panelY + 455f,
                    500f,
                    30f),
                "Quality: " +
                lastResult.quality
                    .ToString("F2"),
                normalStyle);


            // =====================================================
            // CONFIDENCE
            // =====================================================

            GUI.Label(
                new Rect(
                    x,
                    panelY + 490f,
                    500f,
                    30f),
                "Confidence: " +
                lastResult.confidence
                    .ToString("F2"),
                normalStyle);


            // =====================================================
            // REP EVENT
            // =====================================================

            GUI.Label(
                new Rect(
                    x,
                    panelY + 525f,
                    500f,
                    30f),
                "Rep Completed: " +
                IsRepCompleted,
                valueStyle);


            // =====================================================
            // FEEDBACK
            // =====================================================

            GUI.Label(
                new Rect(
                    x,
                    panelY + 560f,
                    500f,
                    70f),
                "Feedback: " +
                lastResult.feedback,
                feedbackStyle);
        }
    }
}