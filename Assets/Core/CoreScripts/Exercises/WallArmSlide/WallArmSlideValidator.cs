using UnityEngine;
using ExerciseGame.Core.Exercise;
using ExerciseGame.Core.Pose;

namespace ExerciseGame.Exercises.WallArmSlide
{
    public class WallArmSlideValidator :
        MonoBehaviour,
        IExerciseValidator
    {
        // =========================================================
        // REFERENCES
        // =========================================================

        [Header("References")]

        [SerializeField]
        private WallArmSlideSettings settings;

        [SerializeField]
        private PoseDataProvider poseProvider;


        // =========================================================
        // DEBUG
        // =========================================================

        [Header("Debug")]

        [SerializeField]
        private bool showDebugLogs = true;


        // =========================================================
        // STATE
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
            get;
            private set;
        }


        public int CompletedReps
        {
            get;
            private set;
        }


        // =========================================================
        // MEASUREMENTS
        // =========================================================

        public float CurrentRightArmElevation
        {
            get;
            private set;
        }


        public float CurrentLeftArmElevation
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


        public float CurrentRightWristAboveShoulder
        {
            get;
            private set;
        }


        public float CurrentLeftWristAboveShoulder
        {
            get;
            private set;
        }


        public float CurrentRightElbowAboveShoulder
        {
            get;
            private set;
        }


        public float CurrentLeftElbowAboveShoulder
        {
            get;
            private set;
        }


        // =========================================================
        // COMPATIBILITY
        // =========================================================

        public float CurrentRightWristHeight
        {
            get
            {
                return CurrentRightWristAboveShoulder;
            }
        }


        public float CurrentLeftWristHeight
        {
            get
            {
                return CurrentLeftWristAboveShoulder;
            }
        }


        public float CurrentRightWristX
        {
            get;
            private set;
        }


        public float CurrentLeftWristX
        {
            get;
            private set;
        }


        // =========================================================
        // ARM STATES
        // =========================================================

        public bool IsRightArmStarting
        {
            get;
            private set;
        }


        public bool IsLeftArmStarting
        {
            get;
            private set;
        }


        public bool IsRightArmRaised
        {
            get;
            private set;
        }


        public bool IsLeftArmRaised
        {
            get;
            private set;
        }


        // =========================================================
        // INTERNAL STATE
        // =========================================================

        private float holdTimer;

        private float returnTimer;

        private int raisedStableFrames;

        private int returnStableFrames;

        private bool targetReached;


        // =========================================================
        // AWAKE
        // =========================================================

        private void Awake()
        {
            if (poseProvider == null)
            {
                poseProvider =
                    FindFirstObjectByType<PoseDataProvider>();
            }


            if (settings == null)
            {
                UnityEngine.Debug.LogError(
                    "[Wall Arm Slide] Settings not assigned.");
            }


            Reset();
        }


        // =========================================================
        // VALIDATE
        // =========================================================

        public ValidationResult Validate(
            PoseFrame pose,
            float deltaTime)
        {
            IsRepCompleted = false;


            // =====================================================
            // SETTINGS
            // =====================================================

            if (settings == null)
            {
                CurrentState =
                    ExerciseState.Invalid;

                IsPoseReady = false;

                return ValidationResult.Invalid(
                    "Wall Arm Slide settings missing.");
            }


            // =====================================================
            // MEASURE
            // =====================================================

            bool measured =
                WallArmSlideMeasurement.TryGetMeasurements(
                    pose,
                    settings.minimumConfidence,

                    out float rightElevation,
                    out float leftElevation,

                    out float rightElbowAngle,
                    out float leftElbowAngle,

                    out float rightWristAboveShoulder,
                    out float leftWristAboveShoulder,

                    out float rightElbowAboveShoulder,
                    out float leftElbowAboveShoulder,

                    out float rightWristX,
                    out float leftWristX,

                    out float rightShoulderX,
                    out float leftShoulderX);


            // =====================================================
            // POSE NOT AVAILABLE
            // =====================================================

            if (!measured)
            {
                IsPoseReady = false;

                CurrentState =
                    ExerciseState.Waiting;

                ResetMovementOnly();


                return ValidationResult.Invalid(
                    "Stand where your whole body is visible.");
            }


            // =====================================================
            // STORE MEASUREMENTS
            // =====================================================

            CurrentRightArmElevation =
                rightElevation;

            CurrentLeftArmElevation =
                leftElevation;


            CurrentRightElbowAngle =
                rightElbowAngle;

            CurrentLeftElbowAngle =
                leftElbowAngle;


            CurrentRightWristAboveShoulder =
                rightWristAboveShoulder;

            CurrentLeftWristAboveShoulder =
                leftWristAboveShoulder;


            // =====================================================
            // FIXED:
            // DIRECTLY USE MEASUREMENTS
            // =====================================================

            CurrentRightElbowAboveShoulder =
                rightElbowAboveShoulder;

            CurrentLeftElbowAboveShoulder =
                leftElbowAboveShoulder;


            CurrentRightWristX =
                rightWristX;

            CurrentLeftWristX =
                leftWristX;


            IsPoseReady = true;


            // =====================================================
            // RIGHT ARM STARTING
            // =====================================================
            //
            // Wrist above shoulder
            // AND
            // Elbow below shoulder
            //
            // =====================================================

            IsRightArmStarting =
                CurrentRightWristAboveShoulder >=
                    settings.startingWristAboveShoulderMin
                &&
                CurrentRightElbowAboveShoulder <=
                    settings.startingElbowAboveShoulderMax;


            // =====================================================
            // LEFT ARM STARTING
            // =====================================================

            IsLeftArmStarting =
                CurrentLeftWristAboveShoulder >=
                    settings.startingWristAboveShoulderMin
                &&
                CurrentLeftElbowAboveShoulder <=
                    settings.startingElbowAboveShoulderMax;


            // =====================================================
            // RIGHT ARM RAISED
            // =====================================================
            //
            // BOTH wrist and elbow above shoulder.
            //
            // =====================================================

            IsRightArmRaised =
                CurrentRightWristAboveShoulder >=
                    settings.raisedWristAboveShoulderMin
                &&
                CurrentRightElbowAboveShoulder >=
                    settings.raisedElbowAboveShoulderMin;


            // =====================================================
            // LEFT ARM RAISED
            // =====================================================

            IsLeftArmRaised =
                CurrentLeftWristAboveShoulder >=
                    settings.raisedWristAboveShoulderMin
                &&
                CurrentLeftElbowAboveShoulder >=
                    settings.raisedElbowAboveShoulderMin;


            // =====================================================
            // BOTH STATES
            // =====================================================

            bool bothStarting =
                IsRightArmStarting &&
                IsLeftArmStarting;


            bool bothRaised =
                IsRightArmRaised &&
                IsLeftArmRaised;


            // =====================================================
            // WAITING
            // =====================================================

            if (CurrentState ==
                ExerciseState.Waiting)
            {
                if (bothStarting)
                {
                    CurrentState =
                        ExerciseState.Ready;

                    raisedStableFrames = 0;

                    targetReached = false;


                    return ValidationResult.Valid(
                        0.5f,
                        1f,
                        "Ready. Raise both arms upward.");
                }


                return ValidationResult.Valid(
                    0.2f,
                    1f,
                    "Get into the starting position.");
            }


            // =====================================================
            // READY
            // =====================================================

            if (CurrentState ==
                ExerciseState.Ready)
            {
                // -------------------------------------------------
                // STILL STARTING
                // -------------------------------------------------

                if (bothStarting)
                {
                    raisedStableFrames = 0;


                    return ValidationResult.Valid(
                        0.3f,
                        1f,
                        "Raise both arms upward.");
                }


                // -------------------------------------------------
                // BOTH ARMS RAISED
                // -------------------------------------------------

                if (bothRaised)
                {
                    raisedStableFrames++;


                    if (raisedStableFrames >=
                        Mathf.Max(
                            1,
                            settings.raisedStableFrames))
                    {
                        CurrentState =
                            ExerciseState.Moving;

                        raisedStableFrames = 0;

                        holdTimer = 0f;

                        targetReached = false;


                        return ValidationResult.Valid(
                            0.8f,
                            1f,
                            "Keep your arms overhead.");
                    }


                    return ValidationResult.Valid(
                        0.6f,
                        1f,
                        "Raise both arms.");
                }


                // -------------------------------------------------
                // NOT RAISED
                // -------------------------------------------------

                raisedStableFrames = 0;


                return ValidationResult.Valid(
                    0.4f,
                    1f,
                    "Raise both wrists and elbows above your shoulders.");
            }


            // =====================================================
            // MOVING / HOLDING
            // =====================================================

            if (CurrentState ==
                ExerciseState.Moving)
            {
                // -------------------------------------------------
                // BOTH ARMS STILL RAISED
                // -------------------------------------------------

                if (bothRaised)
                {
                    targetReached = true;

                    holdTimer += deltaTime;


                    if (holdTimer >=
                        settings.requiredHoldTime)
                    {
                        CurrentState =
                            ExerciseState.Returning;

                        holdTimer = 0f;

                        returnTimer = 0f;

                        returnStableFrames = 0;


                        return ValidationResult.Valid(
                            1f,
                            1f,
                            "Great! Slowly return both arms to the starting position.");
                    }


                    float holdProgress =
                        Mathf.Clamp01(
                            holdTimer /
                            Mathf.Max(
                                0.01f,
                                settings.requiredHoldTime));


                    return ValidationResult.Valid(
                        0.8f +
                        (0.15f * holdProgress),
                        1f,
                        "Hold your arms overhead.");
                }


                // -------------------------------------------------
                // RETURNED BEFORE HOLD COMPLETED
                // -------------------------------------------------

                if (!targetReached)
                {
                    if (bothStarting)
                    {
                        CurrentState =
                            ExerciseState.Ready;


                        return ValidationResult.Valid(
                            0.3f,
                            1f,
                            "Raise both arms again.");
                    }


                    return ValidationResult.Valid(
                        0.5f,
                        1f,
                        "Keep both elbows and wrists above your shoulders.");
                }


                // -------------------------------------------------
                // TARGET WAS REACHED
                // -------------------------------------------------

                return ValidationResult.Valid(
                    0.8f,
                    1f,
                    "Slowly return both arms to the starting position.");
            }


            // =====================================================
            // RETURNING
            // =====================================================

            if (CurrentState ==
                ExerciseState.Returning)
            {
                // -------------------------------------------------
                // NEVER COMPLETE WITHOUT TARGET
                // -------------------------------------------------

                if (!targetReached)
                {
                    CurrentState =
                        ExerciseState.Ready;


                    return ValidationResult.Valid(
                        0.3f,
                        1f,
                        "Raise both arms again.");
                }


                // -------------------------------------------------
                // BOTH ARMS RETURNED TO START
                // -------------------------------------------------

                if (bothStarting)
                {
                    returnStableFrames++;

                    returnTimer += deltaTime;


                    if (returnStableFrames >=
                            Mathf.Max(
                                1,
                                settings.returnStableFrames)
                        &&
                        returnTimer >=
                            settings.requiredReturnTime)
                    {
                        CompleteRep();


                        return ValidationResult.Valid(
                            1f,
                            1f,
                            "Rep completed! Raise both arms again.");
                    }


                    return ValidationResult.Valid(
                        0.9f,
                        1f,
                        "Hold the starting position.");
                }


                // -------------------------------------------------
                // NOT YET RETURNED
                // -------------------------------------------------

                returnStableFrames = 0;

                returnTimer = 0f;


                return ValidationResult.Valid(
                    0.8f,
                    1f,
                    "Return both arms to the starting position.");
            }


            // =====================================================
            // REP CONFIRMED
            // =====================================================

            if (CurrentState ==
                ExerciseState.RepConfirmed)
            {
                return ValidationResult.Valid(
                    1f,
                    1f,
                    "Rep completed!");
            }


            // =====================================================
            // INVALID
            // =====================================================

            if (CurrentState ==
                ExerciseState.Invalid)
            {
                if (bothStarting)
                {
                    CurrentState =
                        ExerciseState.Ready;


                    return ValidationResult.Valid(
                        0.3f,
                        1f,
                        "Ready. Raise both arms upward.");
                }


                return ValidationResult.Invalid(
                    "Return to the starting position.");
            }


            // =====================================================
            // DEFAULT
            // =====================================================

            return ValidationResult.Valid(
                0.5f,
                1f,
                "Continue.");
        }


        // =========================================================
        // COMPLETE REP
        // =========================================================

        private void CompleteRep()
        {
            IsRepCompleted = true;

            CompletedReps++;


            CurrentState =
                ExerciseState.RepConfirmed;


            targetReached = false;

            holdTimer = 0f;

            returnTimer = 0f;

            raisedStableFrames = 0;

            returnStableFrames = 0;


            if (showDebugLogs)
            {
                UnityEngine.Debug.Log(
                    "[Wall Arm Slide] REP COMPLETED = " +
                    CompletedReps);
            }
        }


        // =========================================================
        // CONSUME REP
        // =========================================================

        public void ConsumeRep()
        {
            IsRepCompleted = false;


            if (CurrentState ==
                ExerciseState.RepConfirmed)
            {
                CurrentState =
                    ExerciseState.Ready;
            }
        }


        // =========================================================
        // RESET
        // =========================================================

        public void Reset()
        {
            CurrentState =
                ExerciseState.Waiting;


            IsRepCompleted = false;

            IsPoseReady = false;

            CompletedReps = 0;


            CurrentRightArmElevation = 0f;

            CurrentLeftArmElevation = 0f;


            CurrentRightElbowAngle = 0f;

            CurrentLeftElbowAngle = 0f;


            CurrentRightWristAboveShoulder = 0f;

            CurrentLeftWristAboveShoulder = 0f;


            CurrentRightElbowAboveShoulder = 0f;

            CurrentLeftElbowAboveShoulder = 0f;


            CurrentRightWristX = 0f;

            CurrentLeftWristX = 0f;


            IsRightArmStarting = false;

            IsLeftArmStarting = false;


            IsRightArmRaised = false;

            IsLeftArmRaised = false;


            ResetMovementOnly();
        }


        // =========================================================
        // RESET MOVEMENT
        // =========================================================

        private void ResetMovementOnly()
        {
            holdTimer = 0f;

            returnTimer = 0f;

            raisedStableFrames = 0;

            returnStableFrames = 0;

            targetReached = false;
        }
    }
}