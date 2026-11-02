using ExerciseGame.Core.Pose;

namespace ExerciseGame.Core.Exercise
{
    public interface IExerciseValidator
    {
        void Reset();

        ValidationResult Validate(
            PoseFrame pose,
            float deltaTime
        );

        ExerciseState CurrentState { get; }

        bool IsRepCompleted { get; }

        void ConsumeRep();
    }
}