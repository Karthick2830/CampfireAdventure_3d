namespace ExerciseGame.Core.Exercise
{
    public struct ValidationResult
    {
        public bool isValid;

        public float quality;
        public float confidence;

        public string feedback;

        public ValidationResult(
            bool isValid,
            float quality,
            float confidence,
            string feedback)
        {
            this.isValid = isValid;
            this.quality = quality;
            this.confidence = confidence;
            this.feedback = feedback;
        }

        public static ValidationResult Invalid(
            string feedback)
        {
            return new ValidationResult(
                false,
                0f,
                0f,
                feedback
            );
        }

        public static ValidationResult Valid(
            float quality,
            float confidence,
            string feedback)
        {
            return new ValidationResult(
                true,
                quality,
                confidence,
                feedback
            );
        }
    }
}