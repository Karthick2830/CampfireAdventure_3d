using UnityEngine;
using ExerciseGame.Core.UI;

namespace ExerciseGame.Exercises.StandingLateralObliqueStretch
{
    public class StandingLateralObliqueUIInstructionProvider :
        MonoBehaviour,
        IExerciseUIInstructionProvider
    {
        [SerializeField]
        private StandingLateralObliqueValidator validator;


        private void Awake()
        {
            if (validator == null)
            {
                validator =
                    FindFirstObjectByType<
                        StandingLateralObliqueValidator>();
            }
        }
        

        public string GetInstruction()
        {
            if (validator == null)
            {
                return "GET READY";
            }


            if (validator.ActiveArm == "NONE")
            {
                return "RAISE ONE ARM OVERHEAD";
            }


            if (validator.ActiveArm == "RIGHT")
            {
                return "BEND TO THE LEFT";
            }


            if (validator.ActiveArm == "LEFT")
            {
                return "BEND TO THE RIGHT";
            }


            return "GET READY";
        }
    }
}