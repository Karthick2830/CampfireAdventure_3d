using UnityEngine;
using ExerciseGame.Core.Pose;

namespace ExerciseGame.Exercises.StandingLateralObliqueStretch
{
    public static class StandingLateralObliqueMeasurement
    {
        // =========================================================
        // BOTH ARM MEASUREMENTS
        // =========================================================

        public static bool TryGetMeasurements(
            PoseFrame pose,
            float minimumConfidence,

            out float torsoLateralAngle,

            out float rightArmOverheadAngle,
            out float leftArmOverheadAngle,

            out float rightElbowAngle,
            out float leftElbowAngle)
        {
            torsoLateralAngle = 0f;

            rightArmOverheadAngle = 180f;
            leftArmOverheadAngle = 180f;

            rightElbowAngle = 0f;
            leftElbowAngle = 0f;


            // =====================================================
            // POSE
            // =====================================================

            if (pose == null ||
                !pose.HasPose)
            {
                return false;
            }


            // =====================================================
            // TORSO LANDMARKS
            // =====================================================

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


            // =====================================================
            // PHYSICAL RIGHT ARM
            //
            // Mirrored camera:
            // Physical RIGHT = MediaPipe LEFT
            // =====================================================

            if (!pose.IsUsable(
                    PoseLandmarkId.LeftElbow,
                    minimumConfidence))
                return false;

            if (!pose.IsUsable(
                    PoseLandmarkId.LeftWrist,
                    minimumConfidence))
                return false;


            // =====================================================
            // PHYSICAL LEFT ARM
            //
            // Mirrored camera:
            // Physical LEFT = MediaPipe RIGHT
            // =====================================================

            if (!pose.IsUsable(
                    PoseLandmarkId.RightElbow,
                    minimumConfidence))
                return false;

            if (!pose.IsUsable(
                    PoseLandmarkId.RightWrist,
                    minimumConfidence))
                return false;


            // =====================================================
            // POSITIONS
            // =====================================================

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


            // =====================================================
            // PHYSICAL RIGHT
            // =====================================================

            Vector3 physicalRightShoulder =
                pose.GetPosition(
                    PoseLandmarkId.LeftShoulder);

            Vector3 physicalRightElbow =
                pose.GetPosition(
                    PoseLandmarkId.LeftElbow);

            Vector3 physicalRightWrist =
                pose.GetPosition(
                    PoseLandmarkId.LeftWrist);


            // =====================================================
            // PHYSICAL LEFT
            // =====================================================

            Vector3 physicalLeftShoulder =
                pose.GetPosition(
                    PoseLandmarkId.RightShoulder);

            Vector3 physicalLeftElbow =
                pose.GetPosition(
                    PoseLandmarkId.RightElbow);

            Vector3 physicalLeftWrist =
                pose.GetPosition(
                    PoseLandmarkId.RightWrist);


            // =====================================================
            // BODY CENTERS
            // =====================================================

            Vector3 shoulderCenter =
                (leftShoulder + rightShoulder) * 0.5f;

            Vector3 hipCenter =
                (leftHip + rightHip) * 0.5f;


            // =====================================================
            // TORSO LATERAL ANGLE
            // =====================================================

            Vector2 torso =
                new Vector2(
                    shoulderCenter.x - hipCenter.x,
                    shoulderCenter.y - hipCenter.y
                );

            if (torso.sqrMagnitude < 0.000001f)
                return false;


            torsoLateralAngle =
                Mathf.Atan2(
                    torso.x,
                    torso.y
                ) * Mathf.Rad2Deg;


            // =====================================================
            // PHYSICAL RIGHT ARM OVERHEAD ANGLE
            // =====================================================

            Vector2 rightUpperArm =
                new Vector2(
                    physicalRightElbow.x -
                    physicalRightShoulder.x,

                    physicalRightElbow.y -
                    physicalRightShoulder.y
                );

            if (rightUpperArm.sqrMagnitude <
                0.000001f)
            {
                return false;
            }


            /*
             * IMPORTANT ANGLE CONVENTION
             *
             * We measure the upper arm against UP.
             *
             * ARM OVERHEAD:
             *      ~0°
             *
             * ARM SIDEWAYS:
             *      ~90°
             *
             * ARM DOWN:
             *      ~180°
             *
             * Therefore:
             *
             *      0° - 15° = OVERHEAD
             */

            rightArmOverheadAngle =
                Vector2.Angle(
                    rightUpperArm,
                    Vector2.up);


            // =====================================================
            // PHYSICAL LEFT ARM OVERHEAD ANGLE
            // =====================================================

            Vector2 leftUpperArm =
                new Vector2(
                    physicalLeftElbow.x -
                    physicalLeftShoulder.x,

                    physicalLeftElbow.y -
                    physicalLeftShoulder.y
                );

            if (leftUpperArm.sqrMagnitude <
                0.000001f)
            {
                return false;
            }


            leftArmOverheadAngle =
                Vector2.Angle(
                    leftUpperArm,
                    Vector2.up);


            // =====================================================
            // PHYSICAL RIGHT ELBOW
            // =====================================================

            Vector2 rightElbowToShoulder =
                new Vector2(
                    physicalRightShoulder.x -
                    physicalRightElbow.x,

                    physicalRightShoulder.y -
                    physicalRightElbow.y
                );

            Vector2 rightElbowToWrist =
                new Vector2(
                    physicalRightWrist.x -
                    physicalRightElbow.x,

                    physicalRightWrist.y -
                    physicalRightElbow.y
                );


            if (rightElbowToShoulder.sqrMagnitude <
                    0.000001f ||
                rightElbowToWrist.sqrMagnitude <
                    0.000001f)
            {
                return false;
            }


            rightElbowAngle =
                Vector2.Angle(
                    rightElbowToShoulder,
                    rightElbowToWrist);


            // =====================================================
            // PHYSICAL LEFT ELBOW
            // =====================================================

            Vector2 leftElbowToShoulder =
                new Vector2(
                    physicalLeftShoulder.x -
                    physicalLeftElbow.x,

                    physicalLeftShoulder.y -
                    physicalLeftElbow.y
                );

            Vector2 leftElbowToWrist =
                new Vector2(
                    physicalLeftWrist.x -
                    physicalLeftElbow.x,

                    physicalLeftWrist.y -
                    physicalLeftElbow.y
                );


            if (leftElbowToShoulder.sqrMagnitude <
                    0.000001f ||
                leftElbowToWrist.sqrMagnitude <
                    0.000001f)
            {
                return false;
            }


            leftElbowAngle =
                Vector2.Angle(
                    leftElbowToShoulder,
                    leftElbowToWrist);


            return true;
        }


        // =========================================================
        // BACKWARD COMPATIBILITY
        // =========================================================

        public static bool TryGetMeasurements(
            PoseFrame pose,
            float minimumConfidence,

            out float torsoLateralAngle,
            out float rightArmOverheadAngle,
            out float rightElbowAngle)
        {
            float leftArmOverheadAngle;
            float leftElbowAngle;

            return TryGetMeasurements(
                pose,
                minimumConfidence,

                out torsoLateralAngle,

                out rightArmOverheadAngle,
                out leftArmOverheadAngle,

                out rightElbowAngle,
                out leftElbowAngle);
        }
    }
}