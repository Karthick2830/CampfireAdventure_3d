using UnityEngine;

namespace ExerciseGame.Exercises.WallArmSlide
{
    [CreateAssetMenu(
        fileName = "WallArmSlideSettings",
        menuName = "Exercise/Wall Arm Slide Settings")]
    public class WallArmSlideSettings : ScriptableObject
    {
        // =========================================================
        // GENERAL
        // =========================================================

        [Header("General")]

        [Range(0f, 1f)]
        public float minimumConfidence = 0.5f;


        // =========================================================
        // STARTING POSITION
        // =========================================================

        [Header("Starting Position")]

        [Tooltip(
            "Minimum vertical distance for wrist above shoulder. " +
            "Positive means wrist is above the shoulder line.")]
        public float startingWristAboveShoulderMin = 0.0f;

        [Tooltip(
            "Maximum vertical distance for elbow above shoulder. " +
            "Zero means elbow must be below or exactly at shoulder.")]
        public float startingElbowAboveShoulderMax = 0.0f;


        // =========================================================
        // RAISED POSITION
        // =========================================================

        [Header("Raised / Overhead Position")]

        [Tooltip(
            "Minimum vertical distance for wrist above shoulder.")]
        public float raisedWristAboveShoulderMin = 0.02f;

        [Tooltip(
            "Minimum vertical distance for elbow above shoulder.")]
        public float raisedElbowAboveShoulderMin = 0.02f;


        // =========================================================
        // HOLD
        // =========================================================

        [Header("Hold")]

        [Tooltip(
            "How long both arms must remain in the raised position.")]
        public float requiredHoldTime = 1.0f;

        [Tooltip(
            "Number of consecutive frames required for raised pose.")]
        public int raisedStableFrames = 3;


        // =========================================================
        // RETURN
        // =========================================================

        [Header("Return")]

        [Tooltip(
            "How long both arms must remain in the starting position " +
            "before completing the repetition.")]
        public float requiredReturnTime = 0.5f;

        [Tooltip(
            "Number of consecutive frames required for starting pose.")]
        public int returnStableFrames = 3;


        // =========================================================
        // LEGACY SETTINGS
        // =========================================================
        //
        // Kept so older scripts/assets do not break.
        //
        // These values are NOT used by the new Wall Arm Slide
        // validation logic.
        // =========================================================

        [Header("Legacy Settings - Not Used")]

        public float startingElbowMinAngle = 0f;

        public float startingElbowMaxAngle = 180f;

        public float startingArmElevationMax = 180f;

        public float minimumMovementAngle = 5f;

        public int movementStableFrames = 3;

        public float maximumOverheadAngle = 20f;

        public float minimumElbowExtension = 150f;

        public float minimumWristAboveShoulder = 0.02f;

        public int targetStableFrames = 3;

        public float maximumHorizontalDrift = 0.25f;

        public bool requireBothArms = true;

        public int returnStableFramesLegacy = 3;
    }
}