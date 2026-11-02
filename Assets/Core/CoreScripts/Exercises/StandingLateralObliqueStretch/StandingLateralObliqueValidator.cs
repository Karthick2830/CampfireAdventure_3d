using UnityEngine;
using ExerciseGame.Core.Exercise;
using ExerciseGame.Core.Pose;

namespace ExerciseGame.Exercises.StandingLateralObliqueStretch
{
    public class StandingLateralObliqueValidator :
        MonoBehaviour,
        IExerciseValidator
    {
        // =========================================================
        // REFERENCES
        // =========================================================

        [Header("References")]

        [SerializeField]
        private StandingLateralObliqueSettings settings;

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
        // DEBUG
        // =========================================================

        [Header("Debug")]

        [SerializeField]
        private bool showDebugLogs = false;


        // =========================================================
        // PUBLIC STATE
        // =========================================================

        public ExerciseState CurrentState
        {
            get;
            private set;
        }

        public bool IsRepCompleted
        {
            get;
            private set;
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
        // MEASUREMENTS
        // =========================================================

        public float CurrentTorsoAngle
        {
            get;
            private set;
        }

        public float CurrentRightArmOverheadAngle
        {
            get;
            private set;
        }

        public float CurrentLeftArmOverheadAngle
        {
            get;
            private set;
        }

        public float CurrentRightElbowAngle
        {
            get;
            private set;
        }

        public float CurrentLeftElbowAngle
        {
            get;
            private set;
        }


        // =========================================================
        // ARM STATES
        // =========================================================

        public bool IsRightArmOverhead
        {
            get;
            private set;
        }

        public bool IsLeftArmOverhead
        {
            get;
            private set;
        }

        public string ActiveArm
        {
            get;
            private set;
        }

        public string ExpectedBendDirection
        {
            get;
            private set;
        }

        public string NextArm
        {
            get;
            private set;
        }

        public int CompletedReps
        {
            get;
            private set;
        }


        // =========================================================
        // DEBUG VALUES
        // =========================================================

        [Header("Runtime Debug")]

        [SerializeField]
        private float debugRightArmAngle;

        [SerializeField]
        private float debugLeftArmAngle;

        [SerializeField]
        private bool debugRightOverhead;

        [SerializeField]
        private bool debugLeftOverhead;

        [SerializeField]
        private string debugActiveArm;

        [SerializeField]
        private string debugNextArm;

        [SerializeField]
        private string debugExpectedBend;

        [SerializeField]
        private string debugDetectedBend;

        [SerializeField]
        private float debugHoldTime;

        [SerializeField]
        private int debugTargetFrames;

        [SerializeField]
        private int debugCompletedReps;


        // =========================================================
        // READINESS DEBUG
        // =========================================================

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
        // INTERNAL STATE
        // =========================================================

        private float neutralTimer;
        private float targetHoldTimer;

        private int movementStableFrames;
        private int targetStableFrames;

        private int committedDirection;
        private int detectedDirection;

        private float maximumReachedAngle;

        private bool targetSuccessfullyReached;

        private float previousAngle;

        private PoseReadinessValidator readinessValidator;


        // =========================================================
        // UNITY
        // =========================================================

        private void Awake()
        {
            CreateReadinessValidator();
            Reset();
        }


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

            IsRepCompleted =
                false;

            CurrentTorsoAngle =
                0f;

            CurrentRightArmOverheadAngle =
                180f;

            CurrentLeftArmOverheadAngle =
                180f;

            CurrentRightElbowAngle =
                0f;

            CurrentLeftElbowAngle =
                0f;

            IsRightArmOverhead =
                false;

            IsLeftArmOverhead =
                false;

            ActiveArm =
                "NONE";

            ExpectedBendDirection =
                "NONE";

            // IMPORTANT:
            // Exercise always starts with RIGHT arm.
            NextArm =
                "RIGHT";

            CompletedReps =
                0;

            neutralTimer =
                0f;

            targetHoldTimer =
                0f;

            movementStableFrames =
                0;

            targetStableFrames =
                0;

            committedDirection =
                0;

            detectedDirection =
                0;

            maximumReachedAngle =
                0f;

            targetSuccessfullyReached =
                false;

            previousAngle =
                0f;

            ResetDebug();

            if (readinessValidator == null)
            {
                CreateReadinessValidator();
            }

            readinessValidator.Reset();
        }


        // =========================================================
        // DEBUG RESET
        // =========================================================

        private void ResetDebug()
        {
            debugRightArmAngle = 180f;
            debugLeftArmAngle = 180f;

            debugRightOverhead = false;
            debugLeftOverhead = false;

            debugActiveArm = "NONE";
            debugNextArm = "RIGHT";
            debugExpectedBend = "NONE";
            debugDetectedBend = "NONE";

            debugHoldTime = 0f;
            debugTargetFrames = 0;
            debugCompletedReps = 0;

            debugReadinessState =
                PoseReadinessState.NoPerson;

            debugVisibilityScore = 0f;
            debugFacingScore = 0f;
        }


        // =========================================================
        // VALIDATE
        // =========================================================

        public ValidationResult Validate(
            PoseFrame pose,
            float deltaTime)
        {
            IsRepCompleted =
                false;


            if (settings == null)
            {
                CurrentState =
                    ExerciseState.Waiting;

                return ValidationResult.Invalid(
                    "Standing Lateral Oblique settings missing.");
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

            if (!StandingLateralObliqueMeasurement
                .TryGetMeasurements(
                    pose,
                    settings.minimumConfidence,

                    out float torsoAngle,
                    out float rightArmAngle,
                    out float leftArmAngle,

                    out float rightElbowAngle,
                    out float leftElbowAngle))
            {
                CurrentState =
                    ExerciseState.Waiting;

                return ValidationResult.Invalid(
                    "Tracking body...");
            }


            // =====================================================
            // STORE
            // =====================================================

            float correctedTorsoAngle =
                settings.invertDirection
                    ? -torsoAngle
                    : torsoAngle;

            CurrentTorsoAngle =
                correctedTorsoAngle;

            CurrentRightArmOverheadAngle =
                rightArmAngle;

            CurrentLeftArmOverheadAngle =
                leftArmAngle;

            CurrentRightElbowAngle =
                rightElbowAngle;

            CurrentLeftElbowAngle =
                leftElbowAngle;


            // =====================================================
            // OVERHEAD
            // =====================================================

            /*
             * NEW CONVENTION
             *
             * Arm down:
             *      ~180°
             *
             * Arm sideways:
             *      ~90°
             *
             * Arm overhead:
             *      ~0°
             *
             * Setting:
             *      0° - 15° = overhead
             */

            IsRightArmOverhead =
                rightArmAngle >=
                settings.minimumOverheadAngle;

            IsLeftArmOverhead =
                leftArmAngle >=
                settings.minimumOverheadAngle;


            debugRightArmAngle =
                rightArmAngle;

            debugLeftArmAngle =
                leftArmAngle;

            debugRightOverhead =
                IsRightArmOverhead;

            debugLeftOverhead =
                IsLeftArmOverhead;

            debugNextArm =
                NextArm;


            // =====================================================
            // WAITING - SELECT REQUIRED ARM
            // =====================================================

            if (CurrentState ==
                ExerciseState.Waiting)
            {
                ActiveArm =
                    "NONE";

                ExpectedBendDirection =
                    "NONE";


                // -----------------------------------------------
                // RIGHT IS REQUIRED
                // -----------------------------------------------

                if (NextArm == "RIGHT")
                {
                    if (!IsRightArmOverhead)
                    {
                        debugActiveArm = "NONE";
                        debugExpectedBend = "NONE";

                        return ValidationResult.Invalid(
                            "Raise your RIGHT arm overhead.");
                    }

                    if (rightElbowAngle <
                        settings.minimumElbowExtensionAngle)
                    {
                        return ValidationResult.Invalid(
                            "Keep your RIGHT arm straight.");
                    }

                    ActiveArm =
                        "RIGHT";

                    ExpectedBendDirection =
                        "LEFT";

                    CurrentState =
                        ExerciseState.Ready;

                    previousAngle =
                        correctedTorsoAngle;

                    debugActiveArm =
                        ActiveArm;

                    debugExpectedBend =
                        ExpectedBendDirection;

                    return ValidationResult.Valid(
                        1f,
                        1f,
                        "RIGHT arm overhead. Bend LEFT.");
                }


                // -----------------------------------------------
                // LEFT IS REQUIRED
                // -----------------------------------------------

                if (NextArm == "LEFT")
                {
                    if (!IsLeftArmOverhead)
                    {
                        debugActiveArm = "NONE";
                        debugExpectedBend = "NONE";

                        return ValidationResult.Invalid(
                            "Raise your LEFT arm overhead.");
                    }

                    if (leftElbowAngle <
                        settings.minimumElbowExtensionAngle)
                    {
                        return ValidationResult.Invalid(
                            "Keep your LEFT arm straight.");
                    }

                    ActiveArm =
                        "LEFT";

                    ExpectedBendDirection =
                        "RIGHT";

                    CurrentState =
                        ExerciseState.Ready;

                    previousAngle =
                        correctedTorsoAngle;

                    debugActiveArm =
                        ActiveArm;

                    debugExpectedBend =
                        ExpectedBendDirection;

                    return ValidationResult.Valid(
                        1f,
                        1f,
                        "LEFT arm overhead. Bend RIGHT.");
                }
            }


            // =====================================================
            // ACTIVE ARM GATE
            // =====================================================

            if (ActiveArm == "RIGHT")
            {
                if (!IsRightArmOverhead)
                {
                    CancelAttempt();

                    CurrentState =
                        ExerciseState.Waiting;

                    return ValidationResult.Invalid(
                        "Keep your RIGHT arm overhead.");
                }

                if (rightElbowAngle <
                    settings.minimumElbowExtensionAngle)
                {
                    CancelAttempt();

                    CurrentState =
                        ExerciseState.Waiting;

                    return ValidationResult.Invalid(
                        "Keep your RIGHT arm straight.");
                }
            }
            else if (ActiveArm == "LEFT")
            {
                if (!IsLeftArmOverhead)
                {
                    CancelAttempt();

                    CurrentState =
                        ExerciseState.Waiting;

                    return ValidationResult.Invalid(
                        "Keep your LEFT arm overhead.");
                }

                if (leftElbowAngle <
                    settings.minimumElbowExtensionAngle)
                {
                    CancelAttempt();

                    CurrentState =
                        ExerciseState.Waiting;

                    return ValidationResult.Invalid(
                        "Keep your LEFT arm straight.");
                }
            }
            else
            {
                CurrentState =
                    ExerciseState.Waiting;

                return ValidationResult.Invalid(
                    "Raise your " +
                    NextArm +
                    " arm overhead.");
            }


            // =====================================================
            // TORSO
            // =====================================================

            // =========================================================
            // BEND AMOUNT
            // =========================================================
            //
            // Upright is approximately ±180°.
            //
            // 179°  -> 1° bend
            // 170°  -> 10° bend
            // -170° -> 10° bend
            //
            // This gives us the actual amount of lateral bend.
            //

            float bendAngle =
                180f -
                Mathf.Abs(correctedTorsoAngle);

            bendAngle =
                Mathf.Clamp(
                    bendAngle,
                    0f,
                    180f);


            // =====================================================
            // DIRECTION
            // =====================================================

            detectedDirection =
                GetDirection(
                    correctedTorsoAngle);

            debugDetectedBend =
                DirectionToString(
                    detectedDirection);


            // =====================================================
            // READY
            // =====================================================

            if (CurrentState ==
                ExerciseState.Ready)
            {
                if (bendAngle <=
                    settings.neutralAngle)
                {
                    movementStableFrames =
                        0;

                    committedDirection =
                        0;

                    maximumReachedAngle =
                        0f;

                    previousAngle =
                        correctedTorsoAngle;

                    return ValidationResult.Valid(
                        1f,
                        1f,
                        "Bend " +
                        ExpectedBendDirection);
                }


                // -----------------------------------------------
                // WRONG SIDE
                // -----------------------------------------------

                if (!IsCorrectDirection(
                        correctedTorsoAngle))
                {
                    return ValidationResult.Invalid(
                        "Wrong side. Bend " +
                        ExpectedBendDirection);
                }


                // -----------------------------------------------
                // COMMIT DIRECTION
                // -----------------------------------------------

                if (committedDirection == 0)
                {
                    committedDirection =
                        detectedDirection;
                }


                if (detectedDirection !=
                    committedDirection)
                {
                    return ValidationResult.Invalid(
                        "Keep bending " +
                        ExpectedBendDirection);
                }


                // -----------------------------------------------
                // MINIMUM MOVEMENT
                // -----------------------------------------------

                if (bendAngle <
                    settings.minimumMovementAngle)
                {
                    movementStableFrames =
                        0;

                    return ValidationResult.Valid(
                        0.25f,
                        1f,
                        "Start bending " +
                        ExpectedBendDirection);
                }


                // -----------------------------------------------
                // MOVEMENT STABILITY
                // -----------------------------------------------

                float frameChange =
                    Mathf.Abs(
                        correctedTorsoAngle -
                        previousAngle);

                previousAngle =
                    correctedTorsoAngle;


                if (frameChange >
                    settings.maximumFrameAngleChange)
                {
                    movementStableFrames =
                        0;

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


                // -----------------------------------------------
                // START MOVING
                // -----------------------------------------------

                maximumReachedAngle =
                    bendAngle;

                targetStableFrames =
                    0;

                targetHoldTimer =
                    0f;

                targetSuccessfullyReached =
                    false;

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
                // -----------------------------------------------
                // ACTIVE ARM MUST STAY OVERHEAD
                // -----------------------------------------------

                if (ActiveArm == "RIGHT" &&
                    !IsRightArmOverhead)
                {
                    CancelAttempt();

                    CurrentState =
                        ExerciseState.Waiting;

                    return ValidationResult.Invalid(
                        "Keep your RIGHT arm overhead.");
                }

                if (ActiveArm == "LEFT" &&
                    !IsLeftArmOverhead)
                {
                    CancelAttempt();

                    CurrentState =
                        ExerciseState.Waiting;

                    return ValidationResult.Invalid(
                        "Keep your LEFT arm overhead.");
                }


                // -----------------------------------------------
                // DIRECTION LOCK
                // -----------------------------------------------

                if (detectedDirection !=
                    committedDirection)
                {
                    return ValidationResult.Invalid(
                        "Keep bending " +
                        ExpectedBendDirection);
                }


                // -----------------------------------------------
                // MAXIMUM REACHED
                // -----------------------------------------------

                if (bendAngle >
                    maximumReachedAngle)
                {
                    maximumReachedAngle =
                            bendAngle;
                }


                // -----------------------------------------------
                // TARGET
                // -----------------------------------------------

                if (bendAngle >=
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

                        targetStableFrames =
                            0;

                        targetHoldTimer =
                            0f;

                        neutralTimer =
                            0f;


                        if (showDebugLogs)
                        {
                            UnityEngine.Debug.Log(
                                "[Standing Lateral Oblique] " +
                                "TARGET REACHED | Arm: " +
                                ActiveArm +
                                " | Bend: " +
                                ExpectedBendDirection +
                                " | Angle: " +
                                bendAngle.ToString("F1"));
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


                // -----------------------------------------------
                // PREMATURE RETURN
                // -----------------------------------------------

                if (bendAngle <
                    maximumReachedAngle -
                    settings.maximumAngleReversal)
                {
                    CancelAttempt();

                    CurrentState =
                        ExerciseState.Ready;

                    return ValidationResult.Invalid(
                        "Reach the target before returning.");
                }


                float progress =
                    Mathf.InverseLerp(
                        settings.minimumMovementAngle,
                        settings.targetAngle,
                        bendAngle);


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
                if (!targetSuccessfullyReached)
                {
                    CancelAttempt();

                    CurrentState =
                        ExerciseState.Ready;

                    return ValidationResult.Invalid(
                        "Target was not reached.");
                }


                // -----------------------------------------------
                // ACTIVE ARM MUST REMAIN OVERHEAD
                // -----------------------------------------------

                if (ActiveArm == "RIGHT" &&
                    !IsRightArmOverhead)
                {
                    CancelAttempt();

                    CurrentState =
                        ExerciseState.Waiting;

                    return ValidationResult.Invalid(
                        "Keep your RIGHT arm overhead.");
                }

                if (ActiveArm == "LEFT" &&
                    !IsLeftArmOverhead)
                {
                    CancelAttempt();

                    CurrentState =
                        ExerciseState.Waiting;

                    return ValidationResult.Invalid(
                        "Keep your LEFT arm overhead.");
                }


                // -----------------------------------------------
                // DIRECTION SAFETY
                // -----------------------------------------------

                if (detectedDirection != 0 &&
                    detectedDirection !=
                    committedDirection &&
                    bendAngle >
                    settings.neutralAngle)
                {
                    return ValidationResult.Invalid(
                        "Return straight to center.");
                }


                // -----------------------------------------------
                // RETURN TO CENTER
                // -----------------------------------------------

                if (bendAngle <=
                    settings.neutralAngle)
                {
                    neutralTimer +=
                        deltaTime;


                    if (neutralTimer >=
                        settings.neutralRequiredTime)
                    {
                        // =========================================
                        // REP COMPLETED
                        // =========================================

                        IsRepCompleted =
                            true;

                        CompletedReps++;


                        if (showDebugLogs)
                        {
                            UnityEngine.Debug.Log(
                                "[Standing Lateral Oblique] " +
                                "REP CONFIRMED | Rep: " +
                                CompletedReps +
                                " | Arm: " +
                                ActiveArm +
                                " | Bend: " +
                                ExpectedBendDirection);
                        }


                        // =========================================
                        // SWITCH ARM
                        // =========================================

                        if (ActiveArm == "RIGHT")
                        {
                            NextArm =
                                "LEFT";
                        }
                        else
                        {
                            NextArm =
                                "RIGHT";
                        }


                        targetSuccessfullyReached =
                            false;

                        neutralTimer =
                            0f;

                        movementStableFrames =
                            0;

                        targetStableFrames =
                            0;

                        targetHoldTimer =
                            0f;

                        committedDirection =
                            0;

                        maximumReachedAngle =
                            0f;

                        previousAngle =
                            correctedTorsoAngle;


                        ActiveArm =
                            "NONE";

                        ExpectedBendDirection =
                            "NONE";

                        CurrentState =
                            ExerciseState.Waiting;


                        debugNextArm =
                            NextArm;

                        debugActiveArm =
                            "NONE";

                        debugExpectedBend =
                            "NONE";


                        return ValidationResult.Valid(
                            1f,
                            1f,
                            "Rep completed! Raise your " +
                            NextArm +
                            " arm.");
                    }


                    return ValidationResult.Valid(
                        0.9f,
                        1f,
                        "Return to center.");
                }


                neutralTimer =
                    0f;

                return ValidationResult.Valid(
                    0.8f,
                    1f,
                    "Return to center.");
            }


            return ValidationResult.Valid(
                0.5f,
                1f,
                "Continue.");
        }


     

        // =========================================================
        // DIRECTION
        // =========================================================

        private int GetDirection(float angle)
        {
            // Standing upright
            if (Mathf.Abs(angle) <= settings.neutralAngle)
            {
                return 0;
            }

            // IMPORTANT:
            // With the current MediaPipe camera coordinate setup,
            // positive torso angle = physical LEFT bend
            // negative torso angle = physical RIGHT bend.

            if (angle > 0f)
            {
                return -1; // LEFT
            }

            return 1; // RIGHT
        }


        // =========================================================
        // CORRECT DIRECTION
        // =========================================================

        private bool IsCorrectDirection(
            float angle)
        {
            int detected =
                GetDirection(angle);

            if (detected == 0)
                return false;


            if (ExpectedBendDirection ==
                "LEFT")
            {
                return detected == -1;
            }


            if (ExpectedBendDirection ==
                "RIGHT")
            {
                return detected == 1;
            }


            return false;
        }


        // =========================================================
        // DIRECTION STRING
        // =========================================================

        private string DirectionToString(
            int direction)
        {
            if (direction < 0)
                return "LEFT";

            if (direction > 0)
                return "RIGHT";

            return "NONE";
        }


        // =========================================================
        // CANCEL ATTEMPT
        // =========================================================

        private void CancelAttempt()
        {
            neutralTimer =
                0f;

            targetHoldTimer =
                0f;

            movementStableFrames =
                0;

            targetStableFrames =
                0;

            committedDirection =
                0;

            maximumReachedAngle =
                0f;

            targetSuccessfullyReached =
                false;

            previousAngle =
                0f;

            ActiveArm =
                "NONE";

            ExpectedBendDirection =
                "NONE";

            IsRightArmOverhead =
                false;

            IsLeftArmOverhead =
                false;

            debugActiveArm =
                "NONE";

            debugExpectedBend =
                "NONE";
        }


        // =========================================================
        // CONSUME REP
        // =========================================================

        public void ConsumeRep()
        {
            IsRepCompleted =
                false;
        }
    }
}