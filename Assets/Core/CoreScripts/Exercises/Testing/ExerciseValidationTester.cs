using UnityEngine;
using ExerciseGame.Core.Exercise;
using ExerciseGame.Core.Pose;
using ExerciseGame.Core.UI;
using ExerciseGame.Exercises.SideBend;

namespace ExerciseGame.Exercises.Debug
{
    public class ExerciseValidationTester :
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
        private SideBendValidator sideBendValidator;


        // =========================================================
        // DEBUG SETTINGS
        // =========================================================

        [Header("Debug")]

        [SerializeField]
        private bool logToConsole = true;

        [SerializeField]
        private float logInterval = 0.25f;


        // =========================================================
        // MEASUREMENT
        // =========================================================

        [Header("Measurement")]

        [SerializeField]
        private float measurementConfidence = 0.5f;

        [SerializeField]
        private float forwardBendDepthThreshold = 0.20f;

        [SerializeField]
        private float sideBendMinimumAngle = 5f;


        // =========================================================
        // DIRECTION
        // =========================================================

        [Header("Direction")]

        [Tooltip(
            "Must match the direction correction used by SideBendValidator.")]
        [SerializeField]
        private bool invertDirection = true;


        // =========================================================
        // RUNTIME VALUES
        // =========================================================

        private float logTimer;

        private int repCount;

        private ValidationResult lastResult;

        private float currentAngle;

        private float currentDepth;

        private float currentVertical;

        private bool isSideBend;

        private bool isForwardBend;

        private bool validationPaused;


        // =========================================================
        // IExerciseValidator
        // =========================================================

        public ExerciseState CurrentState
        {
            get
            {
                if (sideBendValidator == null)
                    return default;

                return sideBendValidator.CurrentState;
            }
        }


        public bool IsRepCompleted
        {
            get
            {
                if (sideBendValidator == null)
                    return false;

                return sideBendValidator.IsRepCompleted;
            }
        }


        public ValidationResult Validate(
            PoseFrame pose,
            float deltaTime)
        {
            if (sideBendValidator == null)
                return default;

            lastResult =
                sideBendValidator.Validate(
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
            if (sideBendValidator == null)
                return;

            sideBendValidator.ConsumeRep();
        }


        // =========================================================
        // GENERIC UI FEEDBACK
        // =========================================================

        public ValidationResult LastResult
        {
            get
            {
                return lastResult;
            }
        }


        // =========================================================
        // EXISTING PUBLIC API
        // =========================================================

        public int RepCount
        {
            get
            {
                return repCount;
            }
        }


        public SideBendValidator Validator
        {
            get
            {
                return sideBendValidator;
            }
        }


        // =========================================================
        // RESET VALIDATION
        // =========================================================

        public void ResetValidation()
        {
            repCount = 0;

            logTimer = 0f;

            currentAngle = 0f;

            currentDepth = 0f;

            currentVertical = 0f;

            isSideBend = false;

            isForwardBend = false;

            lastResult = default;

            validationPaused = false;


            if (sideBendValidator != null)
            {
                sideBendValidator.Reset();
            }
        }


        // =========================================================
        // RESET REP COUNTER
        // =========================================================

        public void ResetRepCounter()
        {
            repCount = 0;
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


        public bool IsValidationPaused
        {
            get
            {
                return validationPaused;
            }
        }


        // =========================================================
        // UPDATE
        // =========================================================

        private void Update()
        {
            if (poseProvider == null ||
                sideBendValidator == null)
            {
                return;
            }


            if (validationPaused)
            {
                return;
            }


            PoseFrame pose =
                poseProvider.CurrentPose;


            currentAngle = 0f;

            currentDepth = 0f;

            currentVertical = 0f;

            isSideBend = false;

            isForwardBend = false;


            // =====================================================
            // MEASURE
            // =====================================================

            if (SideBendMeasurement.TryGetMeasurements(
                    pose,
                    measurementConfidence,
                    out float rawAngle,
                    out float depth,
                    out float vertical))
            {
                currentAngle =
                    invertDirection
                        ? -rawAngle
                        : rawAngle;

                currentDepth =
                    depth;

                currentVertical =
                    vertical;


                isSideBend =
                    SideBendMeasurement.IsSideBend(
                        currentAngle,
                        currentDepth,
                        sideBendMinimumAngle);


                isForwardBend =
                    SideBendMeasurement.IsForwardBend(
                        currentAngle,
                        currentDepth,
                        forwardBendDepthThreshold);
            }


            // =====================================================
            // VALIDATION
            // =====================================================

            Validate(
                pose,
                Time.deltaTime);


            // =====================================================
            // LOG
            // =====================================================

            logTimer +=
                Time.deltaTime;


            if (logToConsole &&
                logTimer >= logInterval)
            {
                logTimer = 0f;


                string movement;


                if (isForwardBend)
                {
                    movement =
                        "FORWARD BEND";
                }
                else if (isSideBend)
                {
                    movement =
                        currentAngle < 0f
                            ? "LEFT SIDE BEND"
                            : "RIGHT SIDE BEND";
                }
                else
                {
                    movement =
                        "NEUTRAL / UNKNOWN";
                }


                UnityEngine.Debug.Log(
                    "[Exercise Validation] " +
                    "Angle: " +
                    currentAngle.ToString("F1") +
                    "° | Depth: " +
                    currentDepth.ToString("F2") +
                    " | Vertical: " +
                    currentVertical.ToString("F2") +
                    " | Movement: " +
                    movement +
                    " | State: " +
                    sideBendValidator.CurrentState +
                    " | Expected: " +
                    DirectionToString(
                        sideBendValidator.ExpectedDirection) +
                    " | Detected: " +
                    DirectionToString(
                        sideBendValidator.DetectedDirection) +
                    " | Tracking: " +
                    poseProvider.CurrentTrackingQuality +
                    " | Quality: " +
                    lastResult.quality.ToString("F2") +
                    " | Confidence: " +
                    lastResult.confidence.ToString("F2") +
                    " | Valid: " +
                    lastResult.isValid +
                    " | Feedback: " +
                    lastResult.feedback);
            }


            // =====================================================
            // REP DEBUG
            // =====================================================

            if (sideBendValidator.IsRepCompleted)
            {
                UnityEngine.Debug.Log(
                    "===== REP CONFIRMED ===== " +
                    "VALID REP DETECTED");
            }
        }


        // =========================================================
        // DIRECTION TEXT
        // =========================================================

        private string DirectionToString(
            int direction)
        {
            if (direction < 0)
            {
                return "LEFT";
            }


            if (direction > 0)
            {
                return "RIGHT";
            }


            return "NONE";
        }


        // =========================================================
        // GUI
        // =========================================================

        private void OnGUI()
        {
            if (poseProvider == null ||
                sideBendValidator == null)
            {
                return;
            }


            // =====================================================
            // PANEL POSITION
            // =====================================================

            float panelWidth = 520f;

            float panelHeight = 520f;

            float panelX =
                Screen.width -
                panelWidth -
                20f;

            float panelY = 20f;

            float contentX =
                panelX + 20f;


            // =====================================================
            // STYLES
            // =====================================================

            GUIStyle titleStyle =
                new GUIStyle(
                    GUI.skin.label);

            titleStyle.fontSize = 22;

            titleStyle.fontStyle =
                FontStyle.Bold;


            GUIStyle normalStyle =
                new GUIStyle(
                    GUI.skin.label);

            normalStyle.fontSize = 18;


            GUIStyle angleStyle =
                new GUIStyle(
                    GUI.skin.label);

            angleStyle.fontSize = 22;

            angleStyle.fontStyle =
                FontStyle.Bold;


            GUIStyle movementStyle =
                new GUIStyle(
                    GUI.skin.label);

            movementStyle.fontSize = 20;

            movementStyle.fontStyle =
                FontStyle.Bold;


            GUIStyle repStyle =
                new GUIStyle(
                    GUI.skin.label);

            repStyle.fontSize = 24;

            repStyle.fontStyle =
                FontStyle.Bold;


            // =====================================================
            // MAIN BOX
            // =====================================================

            GUI.Box(
                new Rect(
                    panelX,
                    panelY,
                    panelWidth,
                    panelHeight),
                "");


            // =====================================================
            // TITLE
            // =====================================================

            GUI.Label(
                new Rect(
                    contentX,
                    35,
                    450,
                    35),
                "SIDE BEND VALIDATION",
                titleStyle);


            // =====================================================
            // TRACKING
            // =====================================================

            GUI.Label(
                new Rect(
                    contentX,
                    80,
                    450,
                    30),
                "Tracking: " +
                poseProvider.CurrentTrackingQuality,
                normalStyle);


            // =====================================================
            // STATE
            // =====================================================

            GUI.Label(
                new Rect(
                    contentX,
                    110,
                    450,
                    30),
                "State: " +
                sideBendValidator.CurrentState,
                normalStyle);


            // =====================================================
            // EXPECTED
            // =====================================================

            GUI.Label(
                new Rect(
                    contentX,
                    140,
                    450,
                    30),
                "Expected: " +
                DirectionToString(
                    sideBendValidator.ExpectedDirection),
                normalStyle);


            // =====================================================
            // DETECTED
            // =====================================================

            GUI.Label(
                new Rect(
                    contentX,
                    170,
                    450,
                    30),
                "Detected: " +
                DirectionToString(
                    sideBendValidator.DetectedDirection),
                normalStyle);


            // =====================================================
            // ANGLE
            // =====================================================

            GUI.Label(
                new Rect(
                    contentX,
                    205,
                    450,
                    35),
                "Torso Angle: " +
                currentAngle.ToString("F1") +
                "°",
                angleStyle);


            // =====================================================
            // DEPTH
            // =====================================================

            GUI.Label(
                new Rect(
                    contentX,
                    245,
                    450,
                    30),
                "Depth: " +
                currentDepth.ToString("F2"),
                normalStyle);


            // =====================================================
            // VERTICAL
            // =====================================================

            GUI.Label(
                new Rect(
                    contentX,
                    275,
                    450,
                    30),
                "Vertical: " +
                currentVertical.ToString("F2"),
                normalStyle);


            // =====================================================
            // MOVEMENT
            // =====================================================

            string movement;


            if (isForwardBend)
            {
                movement =
                    "FORWARD BEND";
            }
            else if (isSideBend)
            {
                movement =
                    currentAngle < 0f
                        ? "LEFT SIDE BEND"
                        : "RIGHT SIDE BEND";
            }
            else
            {
                movement =
                    "NEUTRAL / UNKNOWN";
            }


            GUI.Label(
                new Rect(
                    contentX,
                    310,
                    450,
                    35),
                "Movement: " +
                movement,
                movementStyle);


            // =====================================================
            // QUALITY
            // =====================================================

            GUI.Label(
                new Rect(
                    contentX,
                    350,
                    450,
                    30),
                "Quality: " +
                lastResult.quality.ToString("F2"),
                normalStyle);


            // =====================================================
            // CONFIDENCE
            // =====================================================

            GUI.Label(
                new Rect(
                    contentX,
                    380,
                    450,
                    30),
                "Confidence: " +
                lastResult.confidence.ToString("F2"),
                normalStyle);


            // =====================================================
            // REPS
            // =====================================================

            GUI.Label(
                new Rect(
                    contentX,
                    420,
                    450,
                    40),
                "REPS: " +
                repCount,
                repStyle);


            // =====================================================
            // FEEDBACK
            // =====================================================

            GUI.Label(
                new Rect(
                    contentX,
                    465,
                    450,
                    40),
                "Feedback: " +
                lastResult.feedback,
                normalStyle);
        }
    }
}