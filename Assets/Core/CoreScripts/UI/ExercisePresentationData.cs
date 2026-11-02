using UnityEngine;

namespace ExerciseGame.Core.UI
{
    [CreateAssetMenu(
        fileName = "ExercisePresentationData",
        menuName = "Exercise Game/UI/Exercise Presentation Data")]
    public class ExercisePresentationData : ScriptableObject
    {
        [Header("Exercise")]
        public string exerciseName;

        [Header("Instruction")]
        public string instructionDetail;
    }
}