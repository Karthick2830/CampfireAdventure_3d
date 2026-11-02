using UnityEngine;
using ExerciseGame.Core.UI;

namespace ExerciseGame.Exercises.WallArmSlide
{
    public class WallArmSlideUIInstructionProvider :
        MonoBehaviour,
        IExerciseUIInstructionProvider
    {
        [SerializeField]
        private WallArmSlideValidator validator;


        private void Awake()
        {
            if (validator == null)
            {
                validator =
                    FindFirstObjectByType<
                        WallArmSlideValidator>();
            }
        }


        public string GetInstruction()
        {
            if (validator == null)
            {
                return "GET READY";
            }


            switch (validator.CurrentState)
            {
                case ExerciseGame.Core.Exercise.ExerciseState.Waiting:
                    return "GET READY";

                case ExerciseGame.Core.Exercise.ExerciseState.Ready:
                    return "SLIDE BOTH ARMS UP";

                case ExerciseGame.Core.Exercise.ExerciseState.Moving:
                    return "SLOWLY SLIDE BOTH ARMS UP";

                case ExerciseGame.Core.Exercise.ExerciseState.TargetReached:
                case ExerciseGame.Core.Exercise.ExerciseState.Holding:
                    return "HOLD";

                case ExerciseGame.Core.Exercise.ExerciseState.Returning:
                    return "SLOWLY RETURN";

                case ExerciseGame.Core.Exercise.ExerciseState.RepConfirmed:
                    return "REP COMPLETED";

                case ExerciseGame.Core.Exercise.ExerciseState.Invalid:
                    return "RETURN TO START";

                default:
                    return "GET READY";
            }
        }
    }
}