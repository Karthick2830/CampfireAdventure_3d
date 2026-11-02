using UnityEngine;

namespace ExerciseGame.Core.Pose
{
    public class PoseDataProvider : MonoBehaviour
    {
        public static PoseDataProvider Instance { get; private set; }

        [Header("Tracking Settings")]
        [SerializeField]
        [Range(0f, 1f)]
        private float minimumConfidence = 0.5f;

        [Header("Smoothing Settings")]
        [SerializeField]
        [Range(0.01f, 1f)]
        private float smoothingFactor = 0.25f;

        // Raw frame coming directly from MediaPipe
        private PoseFrame rawPose;

        // Smoothed frame used by validators
        private PoseFrame currentPose;

        private PoseSmoother poseSmoother;
        private TrackingQualityEvaluator qualityEvaluator;

        private TrackingQuality trackingQuality = TrackingQuality.NoPerson;

        private int frameCount = 0;
        private long latestTimestamp = 0;

        public PoseFrame RawPose => rawPose;

        public PoseFrame CurrentPose => currentPose;

        public bool HasPose =>
            currentPose != null && currentPose.HasPose;

        public TrackingQuality CurrentTrackingQuality =>
            trackingQuality;

        public float MinimumConfidence =>
            minimumConfidence;

        public int FrameCount =>
            frameCount;

        public long LatestTimestamp =>
            latestTimestamp;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            rawPose = new PoseFrame(33);
            currentPose = new PoseFrame(33);

            poseSmoother = new PoseSmoother(smoothingFactor);
            qualityEvaluator = new TrackingQualityEvaluator();
        }

        public void UpdatePose(
            PoseLandmarkData[] landmarks,
            long timestamp)
        {
            latestTimestamp = timestamp;

            if (landmarks == null || landmarks.Length == 0)
            {
                ClearPose(timestamp);
                return;
            }

            // Store raw MediaPipe pose
            rawPose.SetPose(landmarks, timestamp);

            // Generate smoothed pose
            currentPose = poseSmoother.Process(rawPose);

            // Evaluate tracking quality
            trackingQuality = qualityEvaluator.Evaluate(currentPose);

            frameCount++;
        }

        public void ClearPose(long timestamp)
        {
            latestTimestamp = timestamp;

            rawPose.Clear(timestamp);
            currentPose.Clear(timestamp);

            trackingQuality = TrackingQuality.NoPerson;

            frameCount++;
        }

        public bool IsTrackingReliable()
        {
            return trackingQuality == TrackingQuality.Good ||
                   trackingQuality == TrackingQuality.Excellent;
        }

        public bool HasRequiredLandmarks(
            params PoseLandmarkId[] landmarks)
        {
            if (!HasPose)
                return false;

            foreach (var id in landmarks)
            {
                if (!currentPose.IsUsable(id, minimumConfidence))
                    return false;
            }

            return true;
        }
    }
}