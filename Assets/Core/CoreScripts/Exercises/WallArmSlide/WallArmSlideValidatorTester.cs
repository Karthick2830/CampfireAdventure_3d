using UnityEngine;
using ExerciseGame.Core.Exercise;
using ExerciseGame.Core.Pose;
using ExerciseGame.Core.UI;
using ExerciseGame.Exercises.WallArmSlide;

namespace ExerciseGame.Exercises.WallArmSlide.Debug
{
    public class WallArmSlideValidatorTester :
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
        private WallArmSlideValidator validator;


        // =========================================================
        // DEBUG
        // =========================================================

        [Header("Debug")]

        [SerializeField]
        private bool showDebugGUI = true;

        [SerializeField]
        private bool logToConsole = true;

        [Tooltip("Debug only: let this tester consume reps itself. Leave OFF when ExerciseManager counts reps, otherwise the manager never sees them.")]
        [SerializeField]
        private bool consumeRepsLocally = false;

        [SerializeField]
        private float logInterval = 0.25f;


        // =========================================================
        // RUNTIME
        // =========================================================

        private ValidationResult lastResult;

        private float logTimer;

        private int repCount;


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


        public WallArmSlideValidator Validator
        {
            get
            {
                return validator;
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


        public int RepCount
        {
            get
            {
                return repCount;
            }
        }


        public ExerciseState CurrentState
        {
            get
            {
                if (validator == null)
                    return ExerciseState.Waiting;

                return validator.CurrentState;
            }
        }


        // =========================================================
        // IExerciseValidator
        // =========================================================

        public ValidationResult Validate(
            PoseFrame pose,
            float deltaTime)
        {
            if (validator == null)
            {
                lastResult =
                    ValidationResult.Invalid(
                        "Wall Arm Slide Validator not assigned.");

                return lastResult;
            }


            lastResult =
                validator.Validate(
                    pose,
                    deltaTime);


            return lastResult;
        }


        public void Reset()
        {
            ResetValidation();
        }


        public void ConsumeRep()
        {
            if (validator == null)
                return;


            validator.ConsumeRep();
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
                        WallArmSlideValidator>();
            }
        }


        private void Update()
        {
            if (poseProvider == null ||
                validator == null)
            {
                return;
            }


            PoseFrame pose =
                poseProvider.CurrentPose;


            // =====================================================
            // VALIDATE
            // =====================================================

            Validate(
                pose,
                Time.deltaTime);


            // =====================================================
            // REP
            // =====================================================

            if (validator.IsRepCompleted)
            {
                repCount++;


                UnityEngine.Debug.Log(
                    "===== WALL ARM SLIDE REP ===== " +
                    repCount);


                if (consumeRepsLocally) validator.ConsumeRep();
            }


            // =====================================================
            // CONSOLE DEBUG
            // =====================================================

            if (!logToConsole)
                return;


            logTimer +=
                Time.deltaTime;


            if (logTimer < logInterval)
                return;


            logTimer = 0f;


            UnityEngine.Debug.Log(
                "[Wall Arm Slide] " +

                "State=" +
                validator.CurrentState +

                " | PoseReady=" +
                validator.IsPoseReady +

                " | RIGHT WristAbove=" +
                validator.CurrentRightWristAboveShoulder
                    .ToString("F3") +

                " | RIGHT ElbowAbove=" +
                validator.CurrentRightElbowAboveShoulder
                    .ToString("F3") +

                " | RIGHT Starting=" +
                validator.IsRightArmStarting +

                " | RIGHT Raised=" +
                validator.IsRightArmRaised +

                " | LEFT WristAbove=" +
                validator.CurrentLeftWristAboveShoulder
                    .ToString("F3") +

                " | LEFT ElbowAbove=" +
                validator.CurrentLeftElbowAboveShoulder
                    .ToString("F3") +

                " | LEFT Starting=" +
                validator.IsLeftArmStarting +

                " | LEFT Raised=" +
                validator.IsLeftArmRaised +

                " | REPS=" +
                repCount +

                " | Feedback=" +
                lastResult.feedback
            );
        }


        // =========================================================
        // RESET
        // =========================================================

        public void ResetValidation()
        {
            repCount = 0;

            logTimer = 0f;

            lastResult = default;


            if (validator != null)
            {
                validator.Reset();
            }
        }


        public void ResetRepCounter()
        {
            repCount = 0;
        }


        // =========================================================
        // GUI DEBUG
        // =========================================================

        private void OnGUI()
        {
            if (!showDebugGUI)
                return;


            if (validator == null)
                return;


            float width = 580f;

            float height = 700f;

            float x = 20f;

            float y = 20f;


            // =====================================================
            // BOX
            // =====================================================

            GUI.Box(
                new Rect(
                    x,
                    y,
                    width,
                    height),
                "");


            // =====================================================
            // STYLES
            // =====================================================

            GUIStyle titleStyle =
                new GUIStyle(
                    GUI.skin.label);

            titleStyle.fontSize = 25;

            titleStyle.fontStyle =
                FontStyle.Bold;


            GUIStyle headerStyle =
                new GUIStyle(
                    GUI.skin.label);

            headerStyle.fontSize = 20;

            headerStyle.fontStyle =
                FontStyle.Bold;


            GUIStyle valueStyle =
                new GUIStyle(
                    GUI.skin.label);

            valueStyle.fontSize = 19;


            GUIStyle bigValueStyle =
                new GUIStyle(
                    GUI.skin.label);

            bigValueStyle.fontSize = 25;

            bigValueStyle.fontStyle =
                FontStyle.Bold;


            GUIStyle feedbackStyle =
                new GUIStyle(
                    GUI.skin.label);

            feedbackStyle.fontSize = 19;

            feedbackStyle.wordWrap = true;


            float left =
                x + 20f;

            float currentY =
                y + 15f;


            // =====================================================
            // TITLE
            // =====================================================

            GUI.Label(
                new Rect(
                    left,
                    currentY,
                    width - 40f,
                    35f),
                "WALL ARM SLIDE - DEBUG",
                titleStyle);


            currentY += 45f;


            // =====================================================
            // STATE
            // =====================================================

            GUI.Label(
                new Rect(
                    left,
                    currentY,
                    width - 40f,
                    30f),
                "STATE: " +
                validator.CurrentState,
                bigValueStyle);


            currentY += 35f;


            // =====================================================
            // POSE READY
            // =====================================================

            GUI.Label(
                new Rect(
                    left,
                    currentY,
                    width - 40f,
                    30f),
                "POSE READY: " +
                validator.IsPoseReady,
                valueStyle);


            currentY += 40f;


            // =====================================================
            // RIGHT ARM
            // =====================================================

            GUI.Label(
                new Rect(
                    left,
                    currentY,
                    width - 40f,
                    30f),
                "PHYSICAL RIGHT ARM",
                headerStyle);


            currentY += 32f;


            GUI.Label(
                new Rect(
                    left,
                    currentY,
                    width - 40f,
                    28f),
                "Wrist Above Shoulder: " +
                validator.CurrentRightWristAboveShoulder
                    .ToString("F3"),
                valueStyle);


            currentY += 28f;


            GUI.Label(
                new Rect(
                    left,
                    currentY,
                    width - 40f,
                    28f),
                "Elbow Above Shoulder: " +
                validator.CurrentRightElbowAboveShoulder
                    .ToString("F3"),
                valueStyle);


            currentY += 28f;


            GUI.Label(
                new Rect(
                    left,
                    currentY,
                    width - 40f,
                    28f),
                "Starting: " +
                validator.IsRightArmStarting,
                valueStyle);


            currentY += 28f;


            GUI.Label(
                new Rect(
                    left,
                    currentY,
                    width - 40f,
                    28f),
                "Raised: " +
                validator.IsRightArmRaised,
                valueStyle);


            currentY += 28f;


            GUI.Label(
                new Rect(
                    left,
                    currentY,
                    width - 40f,
                    28f),
                "Elbow Angle: " +
                validator.CurrentRightElbowAngle
                    .ToString("F1") +
                "°",
                valueStyle);


            currentY += 40f;


            // =====================================================
            // LEFT ARM
            // =====================================================

            GUI.Label(
                new Rect(
                    left,
                    currentY,
                    width - 40f,
                    30f),
                "PHYSICAL LEFT ARM",
                headerStyle);


            currentY += 32f;


            GUI.Label(
                new Rect(
                    left,
                    currentY,
                    width - 40f,
                    28f),
                "Wrist Above Shoulder: " +
                validator.CurrentLeftWristAboveShoulder
                    .ToString("F3"),
                valueStyle);


            currentY += 28f;


            GUI.Label(
                new Rect(
                    left,
                    currentY,
                    width - 40f,
                    28f),
                "Elbow Above Shoulder: " +
                validator.CurrentLeftElbowAboveShoulder
                    .ToString("F3"),
                valueStyle);


            currentY += 28f;


            GUI.Label(
                new Rect(
                    left,
                    currentY,
                    width - 40f,
                    28f),
                "Starting: " +
                validator.IsLeftArmStarting,
                valueStyle);


            currentY += 28f;


            GUI.Label(
                new Rect(
                    left,
                    currentY,
                    width - 40f,
                    28f),
                "Raised: " +
                validator.IsLeftArmRaised,
                valueStyle);


            currentY += 28f;


            GUI.Label(
                new Rect(
                    left,
                    currentY,
                    width - 40f,
                    28f),
                "Elbow Angle: " +
                validator.CurrentLeftElbowAngle
                    .ToString("F1") +
                "°",
                valueStyle);


            currentY += 40f;


            // =====================================================
            // REP
            // =====================================================

            GUI.Label(
                new Rect(
                    left,
                    currentY,
                    width - 40f,
                    30f),
                "REPS: " +
                repCount,
                bigValueStyle);


            currentY += 40f;


            // =====================================================
            // TARGET
            // =====================================================

            GUI.Label(
                new Rect(
                    left,
                    currentY,
                    width - 40f,
                    28f),
                "Target Reached: " +
                (validator.CurrentState ==
                 ExerciseState.Returning ||
                 validator.CurrentState ==
                 ExerciseState.RepConfirmed),
                valueStyle);


            currentY += 35f;


            // =====================================================
            // FEEDBACK
            // =====================================================

            GUI.Label(
                new Rect(
                    left,
                    currentY,
                    width - 40f,
                    70f),
                "Feedback: " +
                lastResult.feedback,
                feedbackStyle);
        }
    }
}