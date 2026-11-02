using UnityEngine;

namespace ExerciseGame.Exercises.SideBend
{
    public enum SideBendDirection
    {
        Left,
        Right,
        Both
    }

    public enum SideBendRepMode
    {
        AnySide,
        AlternateSides,
        LeftOnly,
        RightOnly
    }

    public enum SideBendStartingSide
    {
        Left,
        Right
    }

    [CreateAssetMenu(
        fileName = "SideBendSettings",
        menuName = "Exercise Game/Exercises/Side Bend Settings")]
    public class SideBendSettings : ScriptableObject
    {
        // =========================================================
        // DIRECTION
        // =========================================================

        [Header("Direction")]

        [Tooltip(
            "General direction restriction. " +
            "Use Both when using AlternateSides.")]
        public SideBendDirection direction =
            SideBendDirection.Both;


        // =========================================================
        // REP DIRECTION MODE
        // =========================================================

        [Header("Rep Direction")]

        [Tooltip(
            "Controls which side is required for each repetition.")]
        public SideBendRepMode repMode =
            SideBendRepMode.AlternateSides;

        [Tooltip(
            "Which side must be performed for the first repetition " +
            "when AlternateSides is selected.")]
        public SideBendStartingSide startingSide =
            SideBendStartingSide.Left;


        // =========================================================
        // TARGET
        // =========================================================

        [Header("Target")]

        [Tooltip(
            "Minimum torso side-bend angle required to reach the target.")]
        [Range(5f, 60f)]
        public float targetAngle = 30f;

        [Tooltip(
            "Angle required to leave the target state.")]
        [Range(1f, 20f)]
        public float exitTolerance = 5f;


        // =========================================================
        // HOLD
        // =========================================================

        [Header("Hold")]

        [Tooltip(
            "How long the target must remain valid.")]
        [Range(0.1f, 2f)]
        public float requiredHoldTime = 0.3f;

        [Tooltip(
            "Number of stable frames required while holding the target.")]
        [Range(2, 30)]
        public int targetStableFrames = 8;


        // =========================================================
        // NEUTRAL
        // =========================================================

        [Header("Neutral")]

        [Tooltip(
            "Maximum torso angle considered neutral.")]
        [Range(1f, 20f)]
        public float neutralAngle = 8f;

        [Tooltip(
            "How long neutral position must remain stable.")]
        [Range(0.1f, 2f)]
        public float neutralRequiredTime = 0.25f;


        // =========================================================
        // MOVEMENT
        // =========================================================

        [Header("Movement")]

        [Tooltip(
            "Minimum movement required before the bend is considered started.")]
        [Range(3f, 30f)]
        public float minimumMovementAngle = 12f;

        [Tooltip(
            "Maximum angle change allowed between consecutive frames.")]
        [Range(1f, 30f)]
        public float maximumFrameAngleChange = 8f;

        [Tooltip(
            "Number of stable frames required before accepting movement.")]
        [Range(2, 30)]
        public int movementStableFrames = 6;

        [Tooltip(
            "Maximum allowed reversal from the maximum angle before reaching target.")]
        [Range(1f, 20f)]
        public float maximumAngleReversal = 5f;


        // =========================================================
        // FORWARD BEND REJECTION
        // =========================================================

        [Header("Forward Bend Rejection")]

        [Tooltip(
            "Maximum allowed depth-to-vertical ratio. " +
            "Higher values allow more forward/backward movement.")]
        [Range(0.05f, 1f)]
        public float maximumDepthRatio = 0.35f;


        // =========================================================
        // TRACKING
        // =========================================================

        [Header("Tracking")]

        [Range(0f, 1f)]
        public float minimumConfidence = 0.7f;
    }
}