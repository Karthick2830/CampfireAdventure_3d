using UnityEngine;
using ExerciseGame.Core.UI;

namespace ExerciseGame.Exercises.SideBend
{
    public class SideBendUIInstructionProvider :
        MonoBehaviour,
        IExerciseUIInstructionProvider
    {
        [Header("References")]
        [SerializeField]
        private SideBendValidator validator;

        public string GetInstruction()
        {
            if (validator == null)
                return string.Empty;

            switch (validator.ExpectedDirection)
            {
                case -1:
                    return "BEND TO THE LEFT";

                case 1:
                    return "BEND TO THE RIGHT";

                default:
                    return "BEND";
            }
        }

        private void Awake()
        {
            if (validator == null)
                validator = GetComponent<SideBendValidator>();
        }
    }
}