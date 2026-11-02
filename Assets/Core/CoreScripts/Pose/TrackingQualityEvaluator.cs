using UnityEngine;

namespace ExerciseGame.Core.Pose
{
    public class TrackingQualityEvaluator
    {
        private readonly float poorThreshold;
        private readonly float goodThreshold;
        private readonly float excellentThreshold;

        public TrackingQualityEvaluator(
            float poorThreshold = 0.45f,
            float goodThreshold = 0.70f,
            float excellentThreshold = 0.85f)
        {
            this.poorThreshold = poorThreshold;
            this.goodThreshold = goodThreshold;
            this.excellentThreshold = excellentThreshold;
        }

        public TrackingQuality Evaluate(PoseFrame pose)
        {
            if (pose == null || !pose.HasPose)
            {
                return TrackingQuality.NoPerson;
            }

            float confidence = CalculateBodyConfidence(pose);

            if (confidence < poorThreshold)
            {
                return TrackingQuality.Poor;
            }

            if (confidence < goodThreshold)
            {
                return TrackingQuality.Unstable;
            }

            if (confidence < excellentThreshold)
            {
                return TrackingQuality.Good;
            }

            return TrackingQuality.Excellent;
        }

        private float CalculateBodyConfidence(PoseFrame pose)
        {
            PoseLandmarkId[] importantLandmarks =
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

            float total = 0f;
            int validCount = 0;

            foreach (var landmark in importantLandmarks)
            {
                var data = pose.Get(landmark);

                total += data.Confidence;
                validCount++;
            }

            if (validCount == 0)
            {
                return 0f;
            }

            return total / validCount;
        }
    }
}