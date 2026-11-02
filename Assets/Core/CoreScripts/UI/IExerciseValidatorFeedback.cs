using ExerciseGame.Core.Exercise;

namespace ExerciseGame.Core.UI
{
    public interface IExerciseValidatorFeedback
    {
        ValidationResult LastResult { get; }
    }
}