using UnityEngine;

namespace ExerciseGame.Core.Pose
{
    public class PoseSmoother
    {
        private readonly float smoothingFactor;

        private PoseFrame smoothedFrame;

        public PoseSmoother(float smoothingFactor = 0.25f)
        {
            this.smoothingFactor =
                Mathf.Clamp01(smoothingFactor);

            smoothedFrame = new PoseFrame(33);
        }

        public PoseFrame Process(PoseFrame rawFrame)
        {
            if (rawFrame == null)
            {
                return smoothedFrame;
            }

            if (!rawFrame.HasPose)
            {
                smoothedFrame.Clear(
                    rawFrame.Timestamp
                );

                return smoothedFrame;
            }

            for (int i = 0;
                 i < rawFrame.LandmarkCount;
                 i++)
            {
                PoseLandmarkId id =
                    (PoseLandmarkId)i;

                PoseLandmarkData current =
                    rawFrame.Get(id);

                PoseLandmarkData previous =
                    smoothedFrame.Get(id);

                Vector3 smoothedPosition =
                    Vector3.Lerp(
                        previous.normalizedPosition,
                        current.normalizedPosition,
                        smoothingFactor
                    );

                PoseLandmarkData result =
                    new PoseLandmarkData(
                        smoothedPosition,
                        current.visibility,
                        current.presence
                    );

                smoothedFrame.SetLandmark(
                    id,
                    result
                );
            }

            smoothedFrame.SetTrackingState(
                true,
                rawFrame.Timestamp
            );

            return smoothedFrame;
        }
    }
}