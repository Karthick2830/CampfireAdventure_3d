using ExerciseGame.Core.Exercise;
using ExerciseGame.Core.Pose;
using ExerciseGame.Core.UI;
using UnityEngine;

namespace ExerciseGame.Exercises.SideBend
{
    public class SideBendValidator :
        MonoBehaviour,
        IExerciseValidator
    {
        // =========================================================
        // REFERENCES
        // =========================================================

        [Header("References")]

        [SerializeField]
        private SideBendSettings settings;

        [SerializeField]
        private PoseDataProvider poseProvider;


        // =========================================================
        // READINESS
        // =========================================================

        [Header("Common Pose Readiness")]

        [Range(0.5f, 1f)]
        [SerializeField]
        private float minimumJointVisibility = 0.75f;

        [Range(0.5f, 1f)]
        [SerializeField]
        private float minimumFacingScore = 0.75f;


        // =========================================================
        // CAMERA / DIRECTION
        // =========================================================

        [Header("Direction Correction")]

        [Tooltip(
            "Enable this if the physical LEFT movement is reported " +
            "as positive by the pose coordinates.")]
        [SerializeField]
        private bool invertDirection = true;


        // =========================================================
        // DEBUG
        // =========================================================

        [Header("Debug")]

        [SerializeField]
        private bool showDebugLogs = false;


        [Header("Runtime Debug")]

        [SerializeField]
        private float debugRawAngle;

        [SerializeField]
        private float debugCorrectedAngle;

        [SerializeField]
        private float debugDepthRatio;

        [SerializeField]
        private bool debugDepthDominant;

        [SerializeField]
        private float debugMaximumAngle;

        [SerializeField]
        private int debugCommittedDirection;

        [SerializeField]
        private int debugExpectedDirection;

        [SerializeField]
        private int debugDetectedDirection;

        [SerializeField]
        private int debugStableFrames;

        [SerializeField]
        private int debugTargetFrames;

        [SerializeField]
        private float debugHoldTime;

        [SerializeField]
        private int debugCompletedReps;


        [Header("Readiness Debug")]

        [SerializeField]
        private PoseReadinessState debugReadinessState;

        [SerializeField]
        [Range(0f, 1f)]
        private float debugVisibilityScore;

        [SerializeField]
        [Range(0f, 1f)]
        private float debugFacingScore;


        // =========================================================
        // PUBLIC STATE
        // =========================================================

        public ExerciseState CurrentState { get; private set; }

        public bool IsRepCompleted { get; private set; }

        public float CurrentTorsoAngle { get; private set; }

        public float CurrentDepthAmount { get; private set; }

        public float CurrentVerticalAmount { get; private set; }


        // =========================================================
        // PUBLIC READINESS
        // =========================================================

        public PoseReadinessState CurrentReadinessState
        {
            get { return debugReadinessState; }
        }

        public float CurrentVisibilityScore
        {
            get { return debugVisibilityScore; }
        }

        public float CurrentFacingScore
        {
            get { return debugFacingScore; }
        }

        public bool IsPoseReady
        {
            get
            {
                return debugReadinessState ==
                       PoseReadinessState.Ready;
            }
        }


        // =========================================================
        // PUBLIC DIRECTION
        // =========================================================

        public int ExpectedDirection
        {
            get { return expectedDirection; }
        }

        public int DetectedDirection
        {
            get { return detectedDirection; }
        }

        public int CommittedDirection
        {
            get { return committedDirection; }
        }

        public int CompletedReps
        {
            get { return completedReps; }
        }


        // =========================================================
        // INTERNAL STATE
        // =========================================================

        private float neutralTimer;

        private float targetHoldTimer;

        private int movementStableFrames;

        private int targetStableFrames;

        private int committedDirection;

        private int expectedDirection;

        private int detectedDirection;

        private float maximumReachedAngle;

        private bool targetSuccessfullyReached;

        private float previousAngle;

        private int completedReps;


        // =========================================================
        // READINESS
        // =========================================================

        private PoseReadinessValidator readinessValidator;


        // =========================================================
        // UNITY
        // =========================================================

        private void Awake()
        {
            CreateReadinessValidator();
            Reset();
        }


        // =========================================================
        // READINESS
        // =========================================================

        private void CreateReadinessValidator()
        {
            readinessValidator =
                new PoseReadinessValidator(
                    minimumJointVisibility,
                    minimumFacingScore);
        }


        // =========================================================
        // RESET
        // =========================================================

        public void Reset()
        {
            CurrentState =
                ExerciseState.Waiting;

            IsRepCompleted = false;

            CurrentTorsoAngle = 0f;

            CurrentDepthAmount = 0f;

            CurrentVerticalAmount = 0f;

            neutralTimer = 0f;

            targetHoldTimer = 0f;

            movementStableFrames = 0;

            targetStableFrames = 0;

            committedDirection = 0;

            expectedDirection = 0;

            detectedDirection = 0;

            maximumReachedAngle = 0f;

            targetSuccessfullyReached = false;

            previousAngle = 0f;

            completedReps = 0;

            ResetReadinessDebug();

            ResetDebug();

            if (settings != null)
            {
                UpdateExpectedDirection();
            }
        }


        private void ResetReadinessDebug()
        {
            debugReadinessState =
                PoseReadinessState.NoPerson;

            debugVisibilityScore = 0f;

            debugFacingScore = 0f;
        }


        private void ResetDebug()
        {
            debugRawAngle = 0f;

            debugCorrectedAngle = 0f;

            debugDepthRatio = 0f;

            debugDepthDominant = false;

            debugMaximumAngle = 0f;

            debugCommittedDirection = 0;

            debugExpectedDirection = 0;

            debugDetectedDirection = 0;

            debugStableFrames = 0;

            debugTargetFrames = 0;

            debugHoldTime = 0f;

            debugCompletedReps = 0;
        }


        // =========================================================
        // VALIDATE
        // =========================================================

        public ValidationResult Validate(
            PoseFrame pose,
            float deltaTime)
        {
            IsRepCompleted = false;

            if (settings == null)
            {
                CurrentState =
                    ExerciseState.Waiting;

                return ValidationResult.Invalid(
                    "Side Bend settings missing.");
            }


            // =====================================================
            // READINESS
            // =====================================================

            if (readinessValidator == null)
            {
                CreateReadinessValidator();
            }

            PoseReadinessResult readiness =
                readinessValidator.Evaluate(
                    pose,
                    deltaTime);

            debugReadinessState =
                readiness.state;

            debugVisibilityScore =
                readiness.visibilityScore;

            debugFacingScore =
                readiness.facingScore;


            if (!readiness.isReady)
            {
                CancelAttempt();

                CurrentState =
                    ExerciseState.Waiting;

                return ValidationResult.Invalid(
                    readiness.feedback);
            }


            // =====================================================
            // MEASUREMENT
            // =====================================================

            if (!SideBendMeasurement.TryGetMeasurements(
                    pose,
                    settings.minimumConfidence,
                    out float rawAngle,
                    out float depthAmount,
                    out float verticalAmount))
            {
                CancelAttempt();

                CurrentState =
                    ExerciseState.Waiting;

                return ValidationResult.Invalid(
                    "Tracking body...");
            }


            // =====================================================
            // DIRECTION CORRECTION
            // =====================================================

            float correctedAngle =
                invertDirection
                    ? -rawAngle
                    : rawAngle;


            CurrentTorsoAngle =
                correctedAngle;

            CurrentDepthAmount =
                depthAmount;

            CurrentVerticalAmount =
                verticalAmount;


            float absoluteAngle =
                Mathf.Abs(correctedAngle);


            // =====================================================
            // DEPTH
            // =====================================================

            float depthRatio =
                verticalAmount > 0.0001f
                    ? depthAmount / verticalAmount
                    : 0f;

            bool depthDominant =
                depthRatio >
                settings.maximumDepthRatio;


            // =====================================================
            // DIRECTION
            // =====================================================

            detectedDirection =
                GetDirection(correctedAngle);


            // =====================================================
            // DEBUG
            // =====================================================

            UpdateDebug(
                rawAngle,
                correctedAngle,
                depthRatio,
                depthDominant);


            // =====================================================
            // WAITING
            // =====================================================

            if (CurrentState ==
                ExerciseState.Waiting)
            {
                if (absoluteAngle <=
                    settings.neutralAngle)
                {
                    neutralTimer +=
                        deltaTime;

                    if (neutralTimer >=
                        settings.neutralRequiredTime)
                    {
                        neutralTimer = 0f;

                        CurrentState =
                            ExerciseState.Ready;

                        previousAngle =
                            correctedAngle;

                        if (showDebugLogs)
                        {
                            UnityEngine.Debug.Log(
                                "[SideBend] READY | Next: " +
                                DirectionToString(
                                    expectedDirection));
                        }

                        return ValidationResult.Valid(
                            1f,
                            1f,
                            GetReadyFeedback());
                    }

                    return ValidationResult.Valid(
                        0.8f,
                        1f,
                        "Get ready...");
                }

                neutralTimer = 0f;

                return ValidationResult.Invalid(
                    "Stand upright.");
            }


            // =====================================================
            // READY
            // =====================================================

            if (CurrentState ==
                ExerciseState.Ready)
            {
                // -------------------------------------------------
                // NEUTRAL
                // -------------------------------------------------

                if (absoluteAngle <=
                    settings.neutralAngle)
                {
                    movementStableFrames = 0;

                    committedDirection = 0;

                    maximumReachedAngle = 0f;

                    previousAngle =
                        correctedAngle;

                    return ValidationResult.Valid(
                        1f,
                        1f,
                        GetBendFeedback());
                }


                // -------------------------------------------------
                // FORWARD BEND
                // -------------------------------------------------

               /* if (depthDominant)
                {
                    CancelAttempt();

                    CurrentState =
                        ExerciseState.Ready;

                    return ValidationResult.Invalid(
                        "Bend sideways, not forward.");
                }*/


                // -------------------------------------------------
                // REQUIRED DIRECTION
                // -------------------------------------------------

                if (!IsCorrectDirection(
                        correctedAngle))
                {
                    CancelAttempt();

                    CurrentState =
                        ExerciseState.Ready;

                    return ValidationResult.Invalid(
                        GetWrongDirectionFeedback());
                }


                // -------------------------------------------------
                // COMMIT DIRECTION
                // -------------------------------------------------

                if (committedDirection == 0)
                {
                    committedDirection =
                        detectedDirection;

                    if (showDebugLogs)
                    {
                        UnityEngine.Debug.Log(
                            "[SideBend] Direction committed: " +
                            DirectionToString(
                                committedDirection));
                    }
                }


                // -------------------------------------------------
                // DIRECTION LOCK
                // -------------------------------------------------

                if (detectedDirection !=
                    committedDirection)
                {
                    CancelAttempt();

                    CurrentState =
                        ExerciseState.Ready;

                    return ValidationResult.Invalid(
                        "Keep bending in the same direction.");
                }


                // -------------------------------------------------
                // MINIMUM MOVEMENT
                // -------------------------------------------------

                if (absoluteAngle <
                    settings.minimumMovementAngle)
                {
                    movementStableFrames = 0;

                    return ValidationResult.Valid(
                        0.25f,
                        1f,
                        GetBendFeedback());
                }


                // -------------------------------------------------
                // FRAME STABILITY
                // -------------------------------------------------

                float frameChange =
                    Mathf.Abs(
                        correctedAngle -
                        previousAngle);

                previousAngle =
                    correctedAngle;


                if (frameChange >
                    settings.maximumFrameAngleChange)
                {
                    movementStableFrames = 0;

                    return ValidationResult.Invalid(
                        "Move smoothly.");
                }


                movementStableFrames++;


                if (movementStableFrames <
                    settings.movementStableFrames)
                {
                    return ValidationResult.Valid(
                        0.4f,
                        1f,
                        "Keep bending.");
                }


                // -------------------------------------------------
                // START MOVING
                // -------------------------------------------------

                maximumReachedAngle =
                    absoluteAngle;

                targetStableFrames = 0;

                targetHoldTimer = 0f;

                targetSuccessfullyReached = false;

                CurrentState =
                    ExerciseState.Moving;

                return ValidationResult.Valid(
                    0.5f,
                    1f,
                    "Keep bending.");
            }


            // =====================================================
            // MOVING
            // =====================================================

            if (CurrentState ==
                ExerciseState.Moving)
            {
                // -------------------------------------------------
                // DIRECTION LOCK
                // -------------------------------------------------

                if (detectedDirection !=
                    committedDirection)
                {
                    CancelAttempt();

                    CurrentState =
                        ExerciseState.Ready;

                    return ValidationResult.Invalid(
                        "Keep bending in the same direction.");
                }


                // -------------------------------------------------
                // FORWARD BEND
                // -------------------------------------------------

                /*if (depthDominant)
                {
                    return ValidationResult.Invalid(
                        "Bend sideways, not forward.");
                }*/


                // -------------------------------------------------
                // MAXIMUM
                // -------------------------------------------------

                if (absoluteAngle >
                    maximumReachedAngle)
                {
                    maximumReachedAngle =
                        absoluteAngle;
                }


                // -------------------------------------------------
                // TARGET
                // -------------------------------------------------

                if (absoluteAngle >=
                    settings.targetAngle)
                {
                    targetStableFrames++;

                    targetHoldTimer +=
                        deltaTime;

                    if (targetStableFrames >=
                            settings.targetStableFrames &&
                        targetHoldTimer >=
                            settings.requiredHoldTime)
                    {
                        targetSuccessfullyReached =
                            true;

                        CurrentState =
                            ExerciseState.Returning;

                        targetStableFrames = 0;

                        targetHoldTimer = 0f;

                        neutralTimer = 0f;

                        if (showDebugLogs)
                        {
                            UnityEngine.Debug.Log(
                                "[SideBend] TARGET REACHED | Side: " +
                                DirectionToString(
                                    committedDirection) +
                                " | Angle: " +
                                absoluteAngle.ToString("F1"));
                        }

                        return ValidationResult.Valid(
                            1f,
                            1f,
                            "Great! Return to center.");
                    }

                    return ValidationResult.Valid(
                        0.95f,
                        1f,
                        "Hold.");
                }


                // -------------------------------------------------
                // PREMATURE RETURN
                // -------------------------------------------------

                if (absoluteAngle <
                    maximumReachedAngle -
                    settings.maximumAngleReversal)
                {
                    CancelAttempt();

                    CurrentState =
                        ExerciseState.Ready;

                    return ValidationResult.Invalid(
                        "Reach the target before returning.");
                }


                // -------------------------------------------------
                // PROGRESS
                // -------------------------------------------------

                float progress =
                    Mathf.InverseLerp(
                        settings.minimumMovementAngle,
                        settings.targetAngle,
                        absoluteAngle);

                return ValidationResult.Valid(
                    progress,
                    1f,
                    "Keep bending.");
            }


            // =====================================================
            // RETURNING
            // =====================================================

            if (CurrentState ==
                ExerciseState.Returning)
            {
                // -------------------------------------------------
                // SAFETY
                // -------------------------------------------------

                if (!targetSuccessfullyReached)
                {
                    CancelAttempt();

                    CurrentState =
                        ExerciseState.Ready;

                    return ValidationResult.Invalid(
                        "Target was not reached.");
                }


                // -------------------------------------------------
                // DIRECTION SAFETY
                // -------------------------------------------------

                if (detectedDirection != 0 &&
                    detectedDirection !=
                    committedDirection &&
                    absoluteAngle >
                    settings.neutralAngle)
                {
                    CancelAttempt();

                    CurrentState =
                        ExerciseState.Ready;

                    return ValidationResult.Invalid(
                        "Return straight to center.");
                }


                // -------------------------------------------------
                // RETURN TO CENTER
                // -------------------------------------------------

                if (absoluteAngle <=
                    settings.neutralAngle)
                {
                    neutralTimer +=
                        deltaTime;

                    if (neutralTimer >=
                        settings.neutralRequiredTime)
                    {
                        IsRepCompleted = true;

                        completedReps++;

                        if (showDebugLogs)
                        {
                            UnityEngine.Debug.Log(
                                "[SideBend] REP CONFIRMED | Rep: " +
                                completedReps +
                                " | Side: " +
                                DirectionToString(
                                    committedDirection));
                        }

                        UpdateExpectedDirection();

                        targetSuccessfullyReached =
                            false;

                        neutralTimer = 0f;

                        movementStableFrames = 0;

                        targetStableFrames = 0;

                        targetHoldTimer = 0f;

                        committedDirection = 0;

                        maximumReachedAngle = 0f;

                        previousAngle =
                            correctedAngle;

                        CurrentState =
                            ExerciseState.Ready;

                        return ValidationResult.Valid(
                            1f,
                            1f,
                            "Rep completed! Next: " +
                            DirectionToString(
                                expectedDirection));
                    }

                    return ValidationResult.Valid(
                        0.9f,
                        1f,
                        "Return to center.");
                }

                neutralTimer = 0f;

                return ValidationResult.Valid(
                    0.8f,
                    1f,
                    "Return to center.");
            }


            // =====================================================
            // FALLBACK
            // =====================================================

            return ValidationResult.Valid(
                0.5f,
                1f,
                "Continue.");
        }


        // =========================================================
        // GET DIRECTION
        // =========================================================

        private int GetDirection(
            float angle)
        {
            if (Mathf.Abs(angle) <=
                settings.neutralAngle)
            {
                return 0;
            }

            if (angle < 0f)
            {
                return -1;
            }

            return 1;
        }


        // =========================================================
        // CHECK REQUIRED DIRECTION
        // =========================================================

        private bool IsCorrectDirection(
            float angle)
        {
            int detected =
                GetDirection(angle);

            if (detected == 0)
            {
                return false;
            }


            // -----------------------------------------------------
            // Explicit direction setting
            // -----------------------------------------------------

            if (settings.direction ==
                SideBendDirection.Left)
            {
                return detected == -1;
            }


            if (settings.direction ==
                SideBendDirection.Right)
            {
                return detected == 1;
            }


            // -----------------------------------------------------
            // Rep mode
            // -----------------------------------------------------

            switch (settings.repMode)
            {
                case SideBendRepMode.AnySide:

                    return true;


                case SideBendRepMode.AlternateSides:

                    return detected ==
                           expectedDirection;


                case SideBendRepMode.LeftOnly:

                    return detected == -1;


                case SideBendRepMode.RightOnly:

                    return detected == 1;
            }

            return false;
        }


        // =========================================================
        // EXPECTED DIRECTION
        // =========================================================

        private void UpdateExpectedDirection()
        {
            if (settings == null)
            {
                expectedDirection = 0;
                return;
            }


            switch (settings.repMode)
            {
                case SideBendRepMode.LeftOnly:

                    expectedDirection = -1;

                    break;


                case SideBendRepMode.RightOnly:

                    expectedDirection = 1;

                    break;


                case SideBendRepMode.AnySide:

                    expectedDirection = 0;

                    break;


                case SideBendRepMode.AlternateSides:

                    bool firstSideIsLeft =
                        settings.startingSide ==
                        SideBendStartingSide.Left;

                    bool nextIsFirstSide =
                        completedReps % 2 == 0;

                    if (firstSideIsLeft)
                    {
                        expectedDirection =
                            nextIsFirstSide
                                ? -1
                                : 1;
                    }
                    else
                    {
                        expectedDirection =
                            nextIsFirstSide
                                ? 1
                                : -1;
                    }

                    break;
            }
        }


        // =========================================================
        // FEEDBACK
        // =========================================================

        private string GetReadyFeedback()
        {
            if (settings.repMode ==
                SideBendRepMode.AlternateSides)
            {
                return "Ready - bend " +
                       DirectionToString(
                           expectedDirection);
            }

            return "Ready";
        }


        private string GetBendFeedback()
        {
            if (settings.repMode ==
                SideBendRepMode.AlternateSides)
            {
                return "Bend " +
                       DirectionToString(
                           expectedDirection);
            }

            return "Bend to the side";
        }


        private string GetWrongDirectionFeedback()
        {
            if (settings.repMode ==
                SideBendRepMode.AlternateSides)
            {
                return "Wrong side. Bend " +
                       DirectionToString(
                           expectedDirection);
            }

            return "Bend to the required side.";
        }


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

            return "EITHER SIDE";
        }


        // =========================================================
        // CANCEL
        // =========================================================

        private void CancelAttempt()
        {
            neutralTimer = 0f;

            targetHoldTimer = 0f;

            movementStableFrames = 0;

            targetStableFrames = 0;

            committedDirection = 0;

            maximumReachedAngle = 0f;

            targetSuccessfullyReached = false;

            previousAngle = 0f;
        }


        // =========================================================
        // DEBUG
        // =========================================================

        private void UpdateDebug(
            float rawAngle,
            float correctedAngle,
            float depthRatio,
            bool depthDominant)
        {
            debugRawAngle =
                rawAngle;

            debugCorrectedAngle =
                correctedAngle;

            debugDepthRatio =
                depthRatio;

            debugDepthDominant =
                depthDominant;

            debugMaximumAngle =
                maximumReachedAngle;

            debugCommittedDirection =
                committedDirection;

            debugExpectedDirection =
                expectedDirection;

            debugDetectedDirection =
                detectedDirection;

            debugStableFrames =
                movementStableFrames;

            debugTargetFrames =
                targetStableFrames;

            debugHoldTime =
                targetHoldTimer;

            debugCompletedReps =
                completedReps;
        }


        // =========================================================
        // CONSUME REP
        // =========================================================

        public void ConsumeRep()
        {
            IsRepCompleted = false;
        }


    }
}