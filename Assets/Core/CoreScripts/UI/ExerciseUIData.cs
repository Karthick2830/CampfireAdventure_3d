using System;

namespace ExerciseGame.Core.UI
{
    [Serializable]
    public class ExerciseUIData
    {
        // Exercise
        public string ExerciseName;

        // Session
        public int CurrentSet;
        public int TargetSets;
        public int CurrentRep;
        public int TargetReps;
        public float Progress;

        // Instructions
        public string Instruction;
        public string InstructionDetail;
        public string Feedback;

        // State
        public bool IsPoseReady;
        public bool IsPaused;
        public bool IsExerciseCompleted;
    }
} 