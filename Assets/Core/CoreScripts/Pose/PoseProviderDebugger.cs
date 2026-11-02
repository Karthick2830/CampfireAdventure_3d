using UnityEngine;

namespace ExerciseGame.Core.Pose
{
    public class PoseProviderDebugger : MonoBehaviour
    {
        [SerializeField]
        private PoseDataProvider poseProvider;

        private void Update()
        {
            if (poseProvider == null)
                return;

            if (!poseProvider.HasPose)
            {
                Debug.Log("Tracking : NO PERSON");
                return;
            }

            var pose = poseProvider.CurrentPose;

            Vector3 leftShoulder =
                pose.GetPosition(PoseLandmarkId.LeftShoulder);

            Vector3 rightShoulder =
                pose.GetPosition(PoseLandmarkId.RightShoulder);

            Vector3 leftHip =
                pose.GetPosition(PoseLandmarkId.LeftHip);

            Vector3 rightHip =
                pose.GetPosition(PoseLandmarkId.RightHip);

            Debug.Log(
                $"Frame:{poseProvider.FrameCount} | " +
                $"Quality:{poseProvider.CurrentTrackingQuality} | " +
                $"LS:{leftShoulder} RS:{rightShoulder} " +
                $"LH:{leftHip} RH:{rightHip}"
            );
        }
    }
}