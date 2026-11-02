using UnityEngine;

namespace ExerciseGame.Exercises.StandingLateralObliqueStretch
{
    [CreateAssetMenu(
        fileName = "StandingLateralObliqueSettings",
        menuName = "Exercise Game/Exercises/Standing Lateral Oblique Settings")]
    public class StandingLateralObliqueSettings : ScriptableObject
    {
        // =========================================================
        // ARM SETTINGS
        // =========================================================

        [Header("Arm Settings")]

        [Tooltip("Maximum angle from UP for the arm to be considered overhead. 0-15 degrees = overhead.")]
        [Range(150f, 180f)]
        public float minimumOverheadAngle = 165f;

        [Tooltip("Minimum elbow angle required to consider the arm straight.")]
        [Range(90f, 180f)]
        public float minimumElbowExtensionAngle = 150f;


        // =========================================================
        // BEND SETTINGS
        // =========================================================

        [Header("Bend Settings")]

        [Tooltip("Minimum torso angle required to reach the stretch target.")]
        [Range(5f, 60f)]
        public float targetAngle = 30f;

        [Tooltip("Maximum allowed reversal while moving toward the target.")]
        [Range(1f, 20f)]
        public float maximumAngleReversal = 5f;


        // =========================================================
        // HOLD SETTINGS
        // =========================================================

        [Header("Hold Settings")]

        [Tooltip("Time the target position must be held.")]
        [Range(0.1f, 2f)]
        public float requiredHoldTime = 0.3f;

        [Tooltip("Stable frames required while holding the target.")]
        [Range(2, 30)]
        public int targetStableFrames = 8;


        // =========================================================
        // RETURN SETTINGS
        // =========================================================

        [Header("Neutral / Return Settings")]

        [Tooltip("Maximum torso angle considered upright.")]
        [Range(1f, 20f)]
        public float neutralAngle = 8f;

        [Tooltip("Time required to remain upright before completing the rep.")]
        [Range(0.1f, 2f)]
        public float neutralRequiredTime = 0.25f;


        // =========================================================
        // MOVEMENT SETTINGS
        // =========================================================

        [Header("Movement Settings")]

        [Tooltip("Minimum torso movement before the bend begins.")]
        [Range(3f, 30f)]
        public float minimumMovementAngle = 12f;

        [Tooltip("Maximum torso angle change allowed between frames.")]
        [Range(1f, 30f)]
        public float maximumFrameAngleChange = 8f;

        [Tooltip("Stable frames required before movement is accepted.")]
        [Range(2, 30)]
        public int movementStableFrames = 6;


        // =========================================================
        // TRACKING
        // =========================================================

        [Header("Tracking")]

        [Range(0f, 1f)]
        public float minimumConfidence = 0.5f;


        // =========================================================
        // DIRECTION
        // =========================================================

        [Header("Direction")]

        public bool invertDirection = true;


        // =========================================================
        // DEBUG
        // =========================================================

        [Header("Debug")]

        public bool showDebugLogs = false;
    }
}