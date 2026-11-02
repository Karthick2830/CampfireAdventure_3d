using UnityEngine;
using ExerciseGame.Core.Pose;

namespace ExerciseGame.Exercises.WallArmSlide
{
    public static class WallArmSlideMeasurement
    {
        // =========================================================
        // MAIN MEASUREMENT
        // =========================================================

        public static bool TryGetMeasurements(
            PoseFrame pose,
            float confidence,

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
            out float leftShoulderX)
        {
            rightElevation = 0f;
            leftElevation = 0f;

            rightElbowAngle = 0f;
            leftElbowAngle = 0f;

            rightWristAboveShoulder = 0f;
            leftWristAboveShoulder = 0f;

            rightElbowAboveShoulder = 0f;
            leftElbowAboveShoulder = 0f;

            rightWristX = 0f;
            leftWristX = 0f;

            rightShoulderX = 0f;
            leftShoulderX = 0f;


            // =====================================================
            // POSE CHECK
            // =====================================================

            if (pose == null || !pose.HasPose)
                return false;


            // =====================================================
            // PHYSICAL RIGHT ARM
            // =====================================================
            //
            // Because the camera is mirrored:
            //
            // Physical RIGHT
            //     = MediaPipe LEFT
            //
            // =====================================================

            PoseLandmarkId rightShoulder =
                PoseLandmarkId.LeftShoulder;

            PoseLandmarkId rightElbow =
                PoseLandmarkId.LeftElbow;

            PoseLandmarkId rightWrist =
                PoseLandmarkId.LeftWrist;


            // =====================================================
            // PHYSICAL LEFT ARM
            // =====================================================

            PoseLandmarkId leftShoulder =
                PoseLandmarkId.RightShoulder;

            PoseLandmarkId leftElbow =
                PoseLandmarkId.RightElbow;

            PoseLandmarkId leftWrist =
                PoseLandmarkId.RightWrist;


            // =====================================================
            // RIGHT ARM VISIBILITY
            // =====================================================

            if (!pose.IsUsable(
                    rightShoulder,
                    confidence))
            {
                return false;
            }

            if (!pose.IsUsable(
                    rightElbow,
                    confidence))
            {
                return false;
            }

            if (!pose.IsUsable(
                    rightWrist,
                    confidence))
            {
                return false;
            }


            // =====================================================
            // LEFT ARM VISIBILITY
            // =====================================================

            if (!pose.IsUsable(
                    leftShoulder,
                    confidence))
            {
                return false;
            }

            if (!pose.IsUsable(
                    leftElbow,
                    confidence))
            {
                return false;
            }

            if (!pose.IsUsable(
                    leftWrist,
                    confidence))
            {
                return false;
            }


            // =====================================================
            // GET POSITIONS
            // =====================================================

            Vector3 physicalRightShoulder =
                pose.GetPosition(rightShoulder);

            Vector3 physicalRightElbow =
                pose.GetPosition(rightElbow);

            Vector3 physicalRightWrist =
                pose.GetPosition(rightWrist);


            Vector3 physicalLeftShoulder =
                pose.GetPosition(leftShoulder);

            Vector3 physicalLeftElbow =
                pose.GetPosition(leftElbow);

            Vector3 physicalLeftWrist =
                pose.GetPosition(leftWrist);


            // =====================================================
            // SHOULDER REFERENCE
            // =====================================================
            //
            // IMPORTANT:
            //
            // We are using shoulder Y as the reference line.
            //
            // shoulder.y - wrist.y
            //
            // POSITIVE = wrist ABOVE shoulder
            // ZERO     = wrist at shoulder
            // NEGATIVE = wrist BELOW shoulder
            //
            // =====================================================

            rightWristAboveShoulder =
                physicalRightShoulder.y -
                physicalRightWrist.y;

            leftWristAboveShoulder =
                physicalLeftShoulder.y -
                physicalLeftWrist.y;


            // =====================================================
            // ELBOW ABOVE SHOULDER
            // =====================================================
            //
            // Same reference system.
            //
            // shoulder.y - elbow.y
            //
            // POSITIVE = elbow ABOVE shoulder
            // ZERO     = elbow at shoulder
            // NEGATIVE = elbow BELOW shoulder
            //
            // =====================================================

            rightElbowAboveShoulder =
                physicalRightShoulder.y -
                physicalRightElbow.y;

            leftElbowAboveShoulder =
                physicalLeftShoulder.y -
                physicalLeftElbow.y;


            // =====================================================
            // ELBOW ANGLES
            // =====================================================

            rightElbowAngle =
                CalculateElbowAngle(
                    physicalRightShoulder,
                    physicalRightElbow,
                    physicalRightWrist);


            leftElbowAngle =
                CalculateElbowAngle(
                    physicalLeftShoulder,
                    physicalLeftElbow,
                    physicalLeftWrist);


            // =====================================================
            // ARM ELEVATION
            // =====================================================
            //
            // Kept only for existing UI/debug compatibility.
            //
            // NOT used for the new Wall Arm Slide validation.
            //
            // =====================================================

            rightElevation =
                CalculateVerticalElevation(
                    physicalRightShoulder,
                    physicalRightElbow);

            leftElevation =
                CalculateVerticalElevation(
                    physicalLeftShoulder,
                    physicalLeftElbow);


            // =====================================================
            // X VALUES
            // =====================================================

            rightWristX =
                physicalRightWrist.x;

            leftWristX =
                physicalLeftWrist.x;


            rightShoulderX =
                physicalRightShoulder.x;

            leftShoulderX =
                physicalLeftShoulder.x;


            return true;
        }


        // =========================================================
        // STARTING POSITION
        // =========================================================

        public static bool IsStartingPosition(
            float rightWristAboveShoulder,
            float leftWristAboveShoulder,

            float rightElbowAboveShoulder,
            float leftElbowAboveShoulder,

            float wristMinimum,
            float elbowMaximum)
        {
            // -----------------------------------------------------
            // RIGHT
            // -----------------------------------------------------

            bool rightStarting =
                rightWristAboveShoulder >= wristMinimum &&
                rightElbowAboveShoulder <= elbowMaximum;


            // -----------------------------------------------------
            // LEFT
            // -----------------------------------------------------

            bool leftStarting =
                leftWristAboveShoulder >= wristMinimum &&
                leftElbowAboveShoulder <= elbowMaximum;


            return rightStarting &&
                   leftStarting;
        }


        // =========================================================
        // RAISED POSITION
        // =========================================================

        public static bool IsRaisedPosition(
            float rightWristAboveShoulder,
            float leftWristAboveShoulder,

            float rightElbowAboveShoulder,
            float leftElbowAboveShoulder,

            float wristMinimum,
            float elbowMinimum)
        {
            // -----------------------------------------------------
            // RIGHT
            // -----------------------------------------------------

            bool rightRaised =
                rightWristAboveShoulder >= wristMinimum &&
                rightElbowAboveShoulder >= elbowMinimum;


            // -----------------------------------------------------
            // LEFT
            // -----------------------------------------------------

            bool leftRaised =
                leftWristAboveShoulder >= wristMinimum &&
                leftElbowAboveShoulder >= elbowMinimum;


            return rightRaised &&
                   leftRaised;
        }


        // =========================================================
        // HORIZONTAL DRIFT
        // =========================================================

        public static bool IsWithinHorizontalDrift(
            float rightWristX,
            float leftWristX,

            float startingRightWristX,
            float startingLeftWristX,

            float maximumDrift)
        {
            float rightDrift =
                Mathf.Abs(
                    rightWristX -
                    startingRightWristX);

            float leftDrift =
                Mathf.Abs(
                    leftWristX -
                    startingLeftWristX);


            return rightDrift <= maximumDrift &&
                   leftDrift <= maximumDrift;
        }


        // =========================================================
        // LEGACY TARGET METHOD
        // =========================================================
        //
        // Kept for compatibility with existing code.
        // New validation does NOT use this.
        //
        // =========================================================

        public static bool IsTargetPosition(
            float rightElevation,
            float leftElevation,

            float rightElbowAngle,
            float leftElbowAngle,

            float rightWristHeight,
            float leftWristHeight,

            float maximumOverheadAngle,
            float minimumElbowExtension,
            float minimumWristAboveShoulder)
        {
            return false;
        }


        // =========================================================
        // ELBOW ANGLE
        // =========================================================

        private static float CalculateElbowAngle(
            Vector3 shoulder,
            Vector3 elbow,
            Vector3 wrist)
        {
            Vector2 elbowToShoulder =
                new Vector2(
                    shoulder.x - elbow.x,
                    shoulder.y - elbow.y);


            Vector2 elbowToWrist =
                new Vector2(
                    wrist.x - elbow.x,
                    wrist.y - elbow.y);


            if (elbowToShoulder.sqrMagnitude <
                    0.000001f ||
                elbowToWrist.sqrMagnitude <
                    0.000001f)
            {
                return 0f;
            }


            return Vector2.Angle(
                elbowToShoulder,
                elbowToWrist);
        }


        // =========================================================
        // ARM ELEVATION
        // =========================================================

        private static float CalculateVerticalElevation(
            Vector3 shoulder,
            Vector3 elbow)
        {
            Vector2 arm =
                new Vector2(
                    elbow.x - shoulder.x,
                    elbow.y - shoulder.y);


            if (arm.sqrMagnitude <
                0.000001f)
            {
                return 0f;
            }


            return Vector2.Angle(
                Vector2.down,
                arm);
        }
    }
}