namespace ExerciseGame.Core.Exercise
{
    public enum PoseReadinessState
    {
        NoPerson,
        JointsNotVisible,
        NotFrontFacing,
        Ready
    }

    public readonly struct PoseReadinessResult
    {
        public readonly PoseReadinessState state;

        public readonly float visibilityScore;
        public readonly float facingScore;

        public readonly bool isReady;

        public readonly string feedback;

        public PoseReadinessResult(
            PoseReadinessState state,
            float visibilityScore,
            float facingScore,
            string feedback)
        {
            this.state = state;
            this.visibilityScore = visibilityScore;
            this.facingScore = facingScore;
            this.isReady =
                state == PoseReadinessState.Ready;

            this.feedback = feedback;
        }

        public static PoseReadinessResult NoPerson()
        {
            return new PoseReadinessResult(
                PoseReadinessState.NoPerson,
                0f,
                0f,
                "Move into camera view.");
        }

        public static PoseReadinessResult VisibilityFailed(
            float visibilityScore)
        {
            return new PoseReadinessResult(
                PoseReadinessState.JointsNotVisible,
                visibilityScore,
                0f,
                "Make sure all joints are visible.");
        }

        public static PoseReadinessResult FacingFailed(
            float visibilityScore,
            float facingScore)
        {
            return new PoseReadinessResult(
                PoseReadinessState.NotFrontFacing,
                visibilityScore,
                facingScore,
                "Please face the camera.");
        }

        public static PoseReadinessResult Ready(
            float visibilityScore,
            float facingScore)
        {
            return new PoseReadinessResult(
                PoseReadinessState.Ready,
                visibilityScore,
                facingScore,
                "Ready.");
        }
    }
}