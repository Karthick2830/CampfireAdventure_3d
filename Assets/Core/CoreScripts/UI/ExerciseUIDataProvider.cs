using UnityEngine;
using ExerciseGame.Core.Exercise;

namespace ExerciseGame.Core.UI
{
    public class ExerciseUIDataProvider : MonoBehaviour
    {
        // =========================================================
        // REFERENCES
        // =========================================================

        [Header("References")]

        [SerializeField]
        private ExerciseManager exerciseManager;

        /*
         * Generic validator tester.
         *
         * Assign any MonoBehaviour that implements:
         * IExerciseValidator
         *
         * Examples:
         * - ExerciseValidationTester
         * - StandingLateralObliqueValidatorTester
         * - Future exercise testers
         */
        [SerializeField]
        private MonoBehaviour validationTesterComponent;

        [SerializeField]
        private ExercisePresentationData presentationData;

        [SerializeField]
        private MonoBehaviour instructionProviderComponent;


        // =========================================================
        // RUNTIME
        // =========================================================

        private ExerciseUIData currentData;

        private IExerciseValidator validationTester;

        private IExerciseUIInstructionProvider instructionProvider;


        // =========================================================
        // PUBLIC API
        // =========================================================

        public ExerciseUIData CurrentData
        {
            get
            {
                return currentData;
            }
        }


        public IExerciseValidator ValidationTester
        {
            get
            {
                return validationTester;
            }
        }


        // =========================================================
        // UNITY
        // =========================================================

        private void Awake()
        {
            currentData =
                new ExerciseUIData();


            // -----------------------------------------------------
            // Exercise Manager
            // -----------------------------------------------------

            if (exerciseManager == null)
            {
                exerciseManager =
                    FindFirstObjectByType<ExerciseManager>();
            }


            // -----------------------------------------------------
            // Validator Tester
            // -----------------------------------------------------

            ResolveValidationTester();


            // -----------------------------------------------------
            // Instruction Provider
            // -----------------------------------------------------

            ResolveInstructionProvider();
        }


        private void Update()
        {
            Refresh();
        }


        // =========================================================
        // VALIDATOR
        // =========================================================

        private void ResolveValidationTester()
        {
            if (validationTesterComponent == null)
            {
                Debug.LogError(
                    "[ExerciseUIDataProvider] " +
                    "Validation Tester Component is not assigned."
                );

                return;
            }


            validationTester =
                validationTesterComponent
                    as IExerciseValidator;


            if (validationTester == null)
            {
                Debug.LogError(
                    "[ExerciseUIDataProvider] " +
                    validationTesterComponent.name +
                    " does not implement IExerciseValidator."
                );
            }
        }


        // =========================================================
        // INSTRUCTION PROVIDER
        // =========================================================

        private void ResolveInstructionProvider()
        {
            if (instructionProviderComponent == null)
            {
                return;
            }


            instructionProvider =
                instructionProviderComponent
                    as IExerciseUIInstructionProvider;


            if (instructionProvider == null)
            {
                Debug.LogError(
                    "[ExerciseUIDataProvider] " +
                    "Instruction Provider does not implement " +
                    "IExerciseUIInstructionProvider."
                );
            }
        }


        // =========================================================
        // REFRESH
        // =========================================================

        public void Refresh()
        {
            if (currentData == null)
            {
                return;
            }


            UpdateExerciseInfo();

            UpdateSessionInfo();

            UpdateProgress();

            UpdateInstruction();

            UpdateState();
        }


        // =========================================================
        // EXERCISE INFORMATION
        // =========================================================

        private void UpdateExerciseInfo()
        {
            if (presentationData == null)
            {
                return;
            }


            currentData.ExerciseName =
                presentationData.exerciseName;


            currentData.InstructionDetail =
                presentationData.instructionDetail;
        }


        // =========================================================
        // SESSION INFORMATION
        // =========================================================

        private void UpdateSessionInfo()
        {
            if (exerciseManager == null)
            {
                return;
            }


            currentData.CurrentSet =
                exerciseManager.CurrentSet;


            currentData.TargetSets =
                exerciseManager.TargetSets;


            currentData.CurrentRep =
                exerciseManager.CurrentRep;


            currentData.TargetReps =
                exerciseManager.TargetRepsPerSet;
        }


        // =========================================================
        // PROGRESS
        // =========================================================

        private void UpdateProgress()
        {
            if (exerciseManager == null)
            {
                currentData.Progress = 0f;

                return;
            }


            currentData.Progress =
                exerciseManager.GetRepProgress();
        }


        // =========================================================
        // INSTRUCTION
        // =========================================================

        private void UpdateInstruction()
        {
            if (instructionProvider != null)
            {
                currentData.Instruction =
                    instructionProvider.GetInstruction();
            }


            // -----------------------------------------------------
            // Feedback
            // -----------------------------------------------------

            IExerciseValidatorFeedback feedbackProvider =
                validationTesterComponent
                    as IExerciseValidatorFeedback;


            if (feedbackProvider != null)
            {
                currentData.Feedback =
                    feedbackProvider
                        .LastResult
                        .feedback;
            }
            else
            {
                currentData.Feedback =
                    string.Empty;
            }
        }


        // =========================================================
        // STATE
        // =========================================================

        private void UpdateState()
        {
            // -----------------------------------------------------
            // Manager state
            // -----------------------------------------------------

            if (exerciseManager != null)
            {
                currentData.IsPaused =
                    exerciseManager.IsPaused;


                currentData.IsExerciseCompleted =
                    exerciseManager.IsExerciseCompleted;
            }


            // -----------------------------------------------------
            // Pose readiness
            // -----------------------------------------------------
            //
            // This is optional because not every future validator
            // needs to expose IsPoseReady.
            //
            // Do not force exercise-specific properties into
            // IExerciseValidator.
            // -----------------------------------------------------

            currentData.IsPoseReady =
                true;
        }
    }
}