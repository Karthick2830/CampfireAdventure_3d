using UnityEngine;
using ExerciseGame.Core.Pose;

namespace ExerciseGame.Core.Exercise
{
    public class PoseReadinessValidator
    {
        // =========================================================
        // DEFAULT SETTINGS
        // =========================================================

        public const float DefaultMinimumVisibility = 0.75f;

        public const float DefaultMinimumVisibilityWhileTracking = 0.65f;

        public const float DefaultMinimumFacingScore = 0.75f;

        public const float DefaultRequiredReadyTime = 0.5f;

        public const float DefaultVisibilityGraceTime = 0.5f;


        // =========================================================
        // REQUIRED LANDMARKS
        // =========================================================

        private static readonly PoseLandmarkId[] RequiredLandmarks =
        {
            PoseLandmarkId.Nose,

            PoseLandmarkId.LeftShoulder,
            PoseLandmarkId.RightShoulder,

            PoseLandmarkId.LeftElbow,
            PoseLandmarkId.RightElbow,

            PoseLandmarkId.LeftWrist,
            PoseLandmarkId.RightWrist,

            PoseLandmarkId.LeftHip,
            PoseLandmarkId.RightHip,

            PoseLandmarkId.LeftKnee,
            PoseLandmarkId.RightKnee,

            PoseLandmarkId.LeftAnkle,
            PoseLandmarkId.RightAnkle
        };


        // =========================================================
        // SETTINGS
        // =========================================================

        private readonly float minimumVisibility;

        private readonly float minimumVisibilityWhileTracking;

        private readonly float minimumFacingScore;

        private readonly float requiredReadyTime;

        private readonly float visibilityGraceTime;


        // =========================================================
        // STATE
        // =========================================================

        private float readyTimer;

        private float visibilityFailureTimer;

        private bool isReady;


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public PoseReadinessValidator(
            float minimumVisibility =
                DefaultMinimumVisibility,

            float minimumVisibilityWhileTracking =
                DefaultMinimumVisibilityWhileTracking,

            float minimumFacingScore =
                DefaultMinimumFacingScore,

            float requiredReadyTime =
                DefaultRequiredReadyTime,

            float visibilityGraceTime =
                DefaultVisibilityGraceTime)
        {
            this.minimumVisibility =
                Mathf.Clamp01(
                    minimumVisibility);

            this.minimumVisibilityWhileTracking =
                Mathf.Clamp01(
                    minimumVisibilityWhileTracking);

            this.minimumFacingScore =
                Mathf.Clamp01(
                    minimumFacingScore);

            this.requiredReadyTime =
                Mathf.Max(
                    0.01f,
                    requiredReadyTime);

            this.visibilityGraceTime =
                Mathf.Max(
                    0.01f,
                    visibilityGraceTime);

            Reset();
        }


        // =========================================================
        // RESET
        // =========================================================

        public void Reset()
        {
            readyTimer = 0f;

            visibilityFailureTimer = 0f;

            isReady = false;
        }


        // =========================================================
        // MAIN READINESS CHECK
        // =========================================================

        public PoseReadinessResult Evaluate(
            PoseFrame pose,
            float deltaTime = 0.016f)
        {
            deltaTime =
                Mathf.Max(
                    0f,
                    deltaTime);


            // =====================================================
            // NO PERSON
            // =====================================================

            if (pose == null ||
                !pose.HasPose)
            {
                Reset();

                return PoseReadinessResult.NoPerson();
            }


            // =====================================================
            // VISIBILITY
            // =====================================================

            float visibilityScore =
                CalculateVisibilityScore(pose);


            bool strictVisibility =
                AreAllRequiredLandmarksVisible(
                    pose,
                    minimumVisibility);


            // =====================================================
            // ALREADY READY
            // =====================================================

            if (isReady)
            {
                /*
                 * Once the user is ready, don't immediately
                 * invalidate because of MediaPipe flickering.
                 *
                 * We use a lower tracking threshold.
                 */

                bool trackingVisibility =
                    AreAllRequiredLandmarksVisible(
                        pose,
                        minimumVisibilityWhileTracking);


                if (!trackingVisibility)
                {
                    visibilityFailureTimer +=
                        deltaTime;


                    if (visibilityFailureTimer <
                        visibilityGraceTime)
                    {
                        /*
                         * Temporary tracking loss.
                         *
                         * Keep the user READY.
                         */

                        return PoseReadinessResult.Ready(
                            visibilityScore,
                            CalculateFacingScoreSafe(pose));
                    }


                    /*
                     * Tracking has remained bad for too long.
                     */

                    Reset();

                    return PoseReadinessResult.VisibilityFailed(
                        visibilityScore);
                }


                /*
                 * Tracking recovered.
                 */

                visibilityFailureTimer = 0f;


                /*
                 * IMPORTANT:
                 *
                 * Do not continuously use the strict
                 * front-facing check here.
                 *
                 * Side bending changes the body geometry.
                 */

                return PoseReadinessResult.Ready(
                    visibilityScore,
                    CalculateFacingScoreSafe(pose));
            }


            // =====================================================
            // NOT READY YET
            // =====================================================

            if (!strictVisibility)
            {
                readyTimer = 0f;

                return PoseReadinessResult.VisibilityFailed(
                    visibilityScore);
            }


            // =====================================================
            // FRONT-FACING CHECK
            // =====================================================

            float facingScore;

            if (!TryCalculateFacingScore(
                    pose,
                    out facingScore))
            {
                readyTimer = 0f;

                return PoseReadinessResult.FacingFailed(
                    visibilityScore,
                    0f);
            }


            if (facingScore <
                minimumFacingScore)
            {
                readyTimer = 0f;

                return PoseReadinessResult.FacingFailed(
                    visibilityScore,
                    facingScore);
            }


            // =====================================================
            // READY STABILITY TIMER
            // =====================================================

            readyTimer +=
                deltaTime;


            if (readyTimer <
                requiredReadyTime)
            {
                return PoseReadinessResult.FacingFailed(
                    visibilityScore,
                    facingScore);
            }


            // =====================================================
            // USER IS NOW READY
            // =====================================================

            isReady = true;

            visibilityFailureTimer = 0f;


            return PoseReadinessResult.Ready(
                visibilityScore,
                facingScore);
        }


        // =========================================================
        // VISIBILITY ONLY
        // =========================================================

        public bool AreJointsVisible(
            PoseFrame pose)
        {
            if (pose == null ||
                !pose.HasPose)
            {
                return false;
            }


            return AreAllRequiredLandmarksVisible(
                pose,
                minimumVisibility);
        }


        public bool AreJointsTrackable(
            PoseFrame pose)
        {
            if (pose == null ||
                !pose.HasPose)
            {
                return false;
            }


            return AreAllRequiredLandmarksVisible(
                pose,
                minimumVisibilityWhileTracking);
        }


        public float CalculateVisibility(
            PoseFrame pose)
        {
            if (pose == null ||
                !pose.HasPose)
            {
                return 0f;
            }


            return CalculateVisibilityScore(
                pose);
        }


        // =========================================================
        // INDIVIDUAL VISIBILITY CHECK
        // =========================================================

        private bool AreAllRequiredLandmarksVisible(
            PoseFrame pose,
            float threshold)
        {
            for (int i = 0;
                 i < RequiredLandmarks.Length;
                 i++)
            {
                PoseLandmarkData landmark =
                    pose.Get(
                        RequiredLandmarks[i]);


                if (landmark.visibility <
                    threshold)
                {
                    return false;
                }
            }


            return true;
        }


        // =========================================================
        // VISIBILITY SCORE
        // =========================================================

        private float CalculateVisibilityScore(
            PoseFrame pose)
        {
            float total = 0f;

            int count = 0;


            for (int i = 0;
                 i < RequiredLandmarks.Length;
                 i++)
            {
                PoseLandmarkData landmark =
                    pose.Get(
                        RequiredLandmarks[i]);


                total +=
                    Mathf.Clamp01(
                        landmark.visibility);

                count++;
            }


            if (count == 0)
                return 0f;


            return total / count;
        }


        // =========================================================
        // SAFE FACING SCORE
        // =========================================================

        private float CalculateFacingScoreSafe(
            PoseFrame pose)
        {
            float score;

            if (TryCalculateFacingScore(
                    pose,
                    out score))
            {
                return score;
            }


            return 0f;
        }


        // =========================================================
        // FRONT-FACING CALCULATION
        // =========================================================

        private bool TryCalculateFacingScore(
            PoseFrame pose,
            out float facingScore)
        {
            facingScore = 0f;


            if (pose == null ||
                !pose.HasPose)
            {
                return false;
            }


            if (!pose.IsUsable(
                    PoseLandmarkId.LeftShoulder,
                    0.01f))
            {
                return false;
            }


            if (!pose.IsUsable(
                    PoseLandmarkId.RightShoulder,
                    0.01f))
            {
                return false;
            }


            if (!pose.IsUsable(
                    PoseLandmarkId.LeftHip,
                    0.01f))
            {
                return false;
            }


            if (!pose.IsUsable(
                    PoseLandmarkId.RightHip,
                    0.01f))
            {
                return false;
            }


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
            // BODY WIDTH
            // =====================================================

            float shoulderWidth =
                Mathf.Abs(
                    rightShoulder.x -
                    leftShoulder.x);

            float hipWidth =
                Mathf.Abs(
                    rightHip.x -
                    leftHip.x);


            if (shoulderWidth <
                0.0001f ||
                hipWidth <
                0.0001f)
            {
                return false;
            }


            // =====================================================
            // DEPTH DIFFERENCE
            // =====================================================

            float shoulderDepthDifference =
                Mathf.Abs(
                    rightShoulder.z -
                    leftShoulder.z);

            float hipDepthDifference =
                Mathf.Abs(
                    rightHip.z -
                    leftHip.z);


            // =====================================================
            // NORMALIZED ROTATION
            // =====================================================

            float shoulderRotation =
                shoulderDepthDifference /
                shoulderWidth;

            float hipRotation =
                hipDepthDifference /
                hipWidth;


            float rotationAmount =
                (shoulderRotation +
                 hipRotation) *
                0.5f;


            // =====================================================
            // FACING SCORE
            // =====================================================

            facingScore =
                1f -
                Mathf.Clamp01(
                    rotationAmount);


            return float.IsFinite(
                facingScore);
        }


        // =========================================================
        // PROPERTIES
        // =========================================================

        public bool IsReady =>
            isReady;

        public float ReadyTimer =>
            readyTimer;

        public float VisibilityFailureTimer =>
            visibilityFailureTimer;

        public float MinimumVisibility =>
            minimumVisibility;

        public float MinimumVisibilityWhileTracking =>
            minimumVisibilityWhileTracking;

        public float MinimumFacingScore =>
            minimumFacingScore;

        public float RequiredReadyTime =>
            requiredReadyTime;

        public float VisibilityGraceTime =>
            visibilityGraceTime;
    }
}