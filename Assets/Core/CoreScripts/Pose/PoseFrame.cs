using UnityEngine;

namespace ExerciseGame.Core.Pose
{
    public class PoseFrame
    {
        private readonly PoseLandmarkData[] landmarks;

        public bool HasPose { get; private set; }

        public long Timestamp { get; private set; }

        public int LandmarkCount => landmarks.Length;

        public PoseFrame(int landmarkCount)
        {
            landmarks = new PoseLandmarkData[landmarkCount];
            HasPose = false;
        }

        public void SetPose(
            PoseLandmarkData[] source,
            long timestamp)
        {
            int count = Mathf.Min(
                source.Length,
                landmarks.Length
            );

            for (int i = 0; i < count; i++)
            {
                landmarks[i] = source[i];
            }

            HasPose = count > 0;
            Timestamp = timestamp;
        }

        public void Clear(long timestamp)
        {
            HasPose = false;
            Timestamp = timestamp;
        }

        public PoseLandmarkData Get(
            PoseLandmarkId id)
        {
            return landmarks[(int)id];
        }

        public Vector3 GetPosition(
            PoseLandmarkId id)
        {
            return Get(id).normalizedPosition;
        }

        public bool IsUsable(
            PoseLandmarkId id,
            float minimumConfidence)
        {
            return Get(id).IsUsable(
                minimumConfidence
            );
        }

        public void SetLandmark(
            PoseLandmarkId id,
            PoseLandmarkData data)
        {
            landmarks[(int)id] = data;
        }

        public void SetTrackingState(
            bool hasPose,
            long timestamp)
        {
            HasPose = hasPose;
            Timestamp = timestamp;
        }

        public float GetAverageConfidence()
        {
            if (!HasPose ||
                landmarks == null ||
                landmarks.Length == 0)
            {
                return 0f;
            }

            float total = 0f;

            for (int i = 0;
                 i < landmarks.Length;
                 i++)
            {
                total += landmarks[i].Confidence;
            }

            return total / landmarks.Length;
        }
    }
}