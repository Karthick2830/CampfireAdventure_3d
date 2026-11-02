using UnityEngine;
using ExerciseGame.Core.Pose;

namespace ExerciseGame.Exercises.SideBend
{
    public static class SideBendMeasurement
    {
        /// <summary>
        /// Simple BlazePose side-bend measurement.
        ///
        /// We calculate:
        ///     Shoulder line angle
        ///     Hip line angle
        ///
        /// Their difference gives the lateral body tilt.
        ///
        /// Positive = one side
        /// Negative = opposite side
        /// </summary>
        public static bool TryGetMeasurements(
            PoseFrame pose,
            float minimumConfidence,
            out float lateralAngle,
            out float depthAmount,
            out float verticalAmount)
        {
            lateralAngle = 0f;
            depthAmount = 0f;
            verticalAmount = 0f;

            if (pose == null || !pose.HasPose)
                return false;

            if (!pose.IsUsable(
                    PoseLandmarkId.LeftShoulder,
                    minimumConfidence))
                return false;

            if (!pose.IsUsable(
                    PoseLandmarkId.RightShoulder,
                    minimumConfidence))
                return false;

            if (!pose.IsUsable(
                    PoseLandmarkId.LeftHip,
                    minimumConfidence))
                return false;

            if (!pose.IsUsable(
                    PoseLandmarkId.RightHip,
                    minimumConfidence))
                return false;


            Vector3 leftShoulder =
                pose.GetPosition(
                    PoseLandmarkId.LeftShoulder);

            Vector3 rightShoulder =
                pose.GetPosition(
                    PoseLandmarkId.RightShoulder);

            Vector3 leftHip =
                pose.GetPosition(
                    PoseLandmarkId.LeftHip);

            Vector3 rightHip =
                pose.GetPosition(
                    PoseLandmarkId.RightHip);


            // -----------------------------------------------------
            // SHOULDER LINE
            // -----------------------------------------------------

            Vector2 shoulderLine =
                new Vector2(
                    rightShoulder.x - leftShoulder.x,
                    rightShoulder.y - leftShoulder.y
                );

            if (shoulderLine.sqrMagnitude < 0.000001f)
                return false;


            // -----------------------------------------------------
            // HIP LINE
            // -----------------------------------------------------

            Vector2 hipLine =
                new Vector2(
                    rightHip.x - leftHip.x,
                    rightHip.y - leftHip.y
                );

            if (hipLine.sqrMagnitude < 0.000001f)
                return false;


            // -----------------------------------------------------
            // LINE ANGLES
            // -----------------------------------------------------

            float shoulderAngle =
                Mathf.Atan2(
                    shoulderLine.y,
                    Mathf.Abs(shoulderLine.x)
                ) * Mathf.Rad2Deg;

            float hipAngle =
                Mathf.Atan2(
                    hipLine.y,
                    Mathf.Abs(hipLine.x)
                ) * Mathf.Rad2Deg;


            // -----------------------------------------------------
            // SIDE-BEND ANGLE
            // -----------------------------------------------------

            lateralAngle =
                shoulderAngle - hipAngle;


            // -----------------------------------------------------
            // TORSO INFORMATION
            // -----------------------------------------------------

            Vector3 shoulderCenter =
                (leftShoulder + rightShoulder) * 0.5f;

            Vector3 hipCenter =
                (leftHip + rightHip) * 0.5f;

            Vector3 torso =
                shoulderCenter - hipCenter;

            float torsoLength =
                torso.magnitude;

            if (torsoLength < 0.0001f)
                return false;


            verticalAmount =
                Mathf.Abs(torso.y) /
                torsoLength;


            depthAmount =
                Mathf.Abs(torso.z) /
                torsoLength;


            // -----------------------------------------------------
            // REMOVE SMALL TRACKING NOISE
            // -----------------------------------------------------

            if (Mathf.Abs(lateralAngle) < 2f)
                lateralAngle = 0f;


            lateralAngle =
                Mathf.Clamp(
                    lateralAngle,
                    -90f,
                    90f);


            return
                float.IsFinite(lateralAngle) &&
                float.IsFinite(depthAmount) &&
                float.IsFinite(verticalAmount);
        }


        public static bool TryGetTorsoAngle(
            PoseFrame pose,
            float minimumConfidence,
            out float angle)
        {
            return TryGetMeasurements(
                pose,
                minimumConfidence,
                out angle,
                out _,
                out _);
        }


        public static bool IsSideBend(
            float lateralAngle,
            float depthAmount,
            float minimumAngle = 5f)
        {
            return Mathf.Abs(lateralAngle) >= minimumAngle;
        }


        public static bool IsForwardBend(
            float lateralAngle,
            float depthAmount,
            float depthThreshold = 0.20f)
        {
            return false;
        }
    }
}