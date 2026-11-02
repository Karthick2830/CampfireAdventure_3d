using UnityEngine;

namespace ExerciseGame.Core.Pose
{
    [System.Serializable]
    public struct PoseLandmarkData
    {
        public Vector3 normalizedPosition;

        public float visibility;
        public float presence;

        public PoseLandmarkData(
            Vector3 normalizedPosition,
            float visibility,
            float presence)
        {
            this.normalizedPosition = normalizedPosition;
            this.visibility = visibility;
            this.presence = presence;
        }

        public float Confidence
        {
            get
            {
                return Mathf.Min(visibility, presence);
            }
        }

        public bool IsUsable(float minimumConfidence)
        {
            return Confidence >= minimumConfidence;
        }
    }
}