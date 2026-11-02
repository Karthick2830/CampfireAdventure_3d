using UnityEngine;
using ExerciseGame.Core.Exercise;

namespace ExerciseGame.Core.Exercise
{
    /// <summary>
    /// Generic exercise progression manager.
    ///
    /// Responsibilities:
    /// - Target reps per set
    /// - Target number of sets
    /// - Current rep
    /// - Current set
    /// - Start / pause / resume / stop
    /// - Detect completed repetitions
    /// - Detect set completion
    /// - Detect exercise completion
    ///
    /// Validation itself is handled by IExerciseValidator.
    /// </summary>
    public class ExerciseManager : MonoBehaviour
    {
        // =========================================================
        // REFERENCES
        // =========================================================

        [Header("References")]

        [Tooltip(
            "Component implementing IExerciseValidator.")]
        [SerializeField]
        private MonoBehaviour validationTesterComponent;


        private IExerciseValidator validationTester;


        // =========================================================
        // EXERCISE TARGET
        // =========================================================

        [Header("Exercise Target")]

        [Tooltip(
            "Number of repetitions required in each set.")]
        [Min(1)]
        [SerializeField]
        private int targetRepsPerSet = 10;


        [Tooltip(
            "Number of sets required to complete the exercise.")]
        [Min(1)]
        [SerializeField]
        private int targetSets = 3;


        // =========================================================
        // STARTUP
        // =========================================================

        [Header("Startup")]

        [Tooltip(
            "Automatically start the exercise when the scene starts.")]
        [SerializeField]
        private bool startAutomatically = true;


        // =========================================================
        // DEBUG
        // =========================================================

        [Header("Debug")]

        [SerializeField]
        private bool showDebugLogs = true;


        // =========================================================
        // RUNTIME STATE
        // =========================================================

        private int currentRep;

        // Reps completed since the exercise started (does not reset between sets).
        private int totalReps;

        /// <summary>Raised once per validated rep, with the running total since StartExercise.</summary>
        public event System.Action<int> RepCompleted;

        public int TotalReps => totalReps;

        private int currentSet;

        private bool isRunning;

        private bool isPaused;

        private bool exerciseCompleted;


        // =========================================================
        // PUBLIC PROPERTIES
        // =========================================================

        public int CurrentRep =>
            currentRep;


        public int CurrentSet =>
            currentSet;


        public int TargetRepsPerSet =>
            targetRepsPerSet;


        public int TargetSets =>
            targetSets;


        public bool IsRunning =>
            isRunning;


        public bool IsPaused =>
            isPaused;


        public bool IsExerciseCompleted =>
            exerciseCompleted;


        public IExerciseValidator ValidationTester =>
            validationTester;


        // =========================================================
        // UNITY
        // =========================================================

        private void Awake()
        {
            ResolveValidator();
        }


        private void Start()
        {
            if (startAutomatically)
            {
                StartExercise();
            }
        }


        private void Update()
        {
            if (!isRunning)
                return;

            if (isPaused)
                return;

            if (exerciseCompleted)
                return;

            if (validationTester == null)
                return;


            CheckForRepCompletion();
        }


        // =========================================================
        // RESOLVE VALIDATOR
        // =========================================================

        private void ResolveValidator()
        {
            if (validationTesterComponent == null)
            {
                Debug.LogError(
                    "[ExerciseManager] " +
                    "Validation Tester Component is not assigned.");

                return;
            }


            validationTester =
                validationTesterComponent
                    as IExerciseValidator;


            if (validationTester == null)
            {
                Debug.LogError(
                    "[ExerciseManager] " +
                    "Assigned component does not implement " +
                    "IExerciseValidator.");
            }
        }


        // =========================================================
        // START EXERCISE
        // =========================================================

        public void StartExercise()
        {
            if (validationTester == null)
            {
                ResolveValidator();
            }


            if (validationTester == null)
            {
                Debug.LogError(
                    "[ExerciseManager] " +
                    "Cannot start exercise. " +
                    "No IExerciseValidator assigned.");

                return;
            }


            currentRep = 0;

            currentSet = 1;

            totalReps = 0;

            isRunning = true;

            isPaused = false;

            exerciseCompleted = false;


            validationTester.Reset();


            if (showDebugLogs)
            {
                Debug.Log(
                    "[ExerciseManager] =============================");

                Debug.Log(
                    "[ExerciseManager] EXERCISE STARTED");

                Debug.Log(
                    $"[ExerciseManager] " +
                    $"Set: {currentSet}/{targetSets}");

                Debug.Log(
                    $"[ExerciseManager] " +
                    $"Rep: {currentRep}/{targetRepsPerSet}");

                Debug.Log(
                    "[ExerciseManager] =============================");
            }
        }


        // =========================================================
        // PAUSE
        // =========================================================

        public void PauseExercise()
        {
            if (!isRunning)
                return;

            if (exerciseCompleted)
                return;

            if (isPaused)
                return;


            isPaused = true;


            // Pause the concrete tester if it supports it.
            if (validationTesterComponent != null)
            {
                validationTesterComponent.SendMessage(
                    "PauseValidation",
                    SendMessageOptions.DontRequireReceiver);
            }


            if (showDebugLogs)
            {
                Debug.Log(
                    "[ExerciseManager] Exercise paused.");
            }
        }


        // =========================================================
        // RESUME
        // =========================================================

        public void ResumeExercise()
        {
            if (!isRunning)
                return;

            if (exerciseCompleted)
                return;

            if (!isPaused)
                return;


            isPaused = false;


            // Resume the concrete tester if it supports it.
            if (validationTesterComponent != null)
            {
                validationTesterComponent.SendMessage(
                    "ResumeValidation",
                    SendMessageOptions.DontRequireReceiver);
            }


            if (showDebugLogs)
            {
                Debug.Log(
                    "[ExerciseManager] Exercise resumed.");
            }
        }


        // =========================================================
        // TOGGLE PAUSE
        // =========================================================

        public void TogglePause()
        {
            if (isPaused)
            {
                ResumeExercise();
            }
            else
            {
                PauseExercise();
            }
        }


        // =========================================================
        // END EXERCISE
        // =========================================================

        public void EndExercise()
        {
            if (!isRunning)
                return;


            isRunning = false;

            isPaused = false;


            if (validationTesterComponent != null)
            {
                validationTesterComponent.SendMessage(
                    "PauseValidation",
                    SendMessageOptions.DontRequireReceiver);
            }


            if (showDebugLogs)
            {
                Debug.Log(
                    "[ExerciseManager] =============================");

                Debug.Log(
                    "[ExerciseManager] EXERCISE ENDED");

                Debug.Log(
                    $"[ExerciseManager] " +
                    $"Set: {currentSet}/{targetSets}");

                Debug.Log(
                    $"[ExerciseManager] " +
                    $"Rep: {currentRep}/{targetRepsPerSet}");

                Debug.Log(
                    "[ExerciseManager] =============================");
            }
        }


        // =========================================================
        // REP DETECTION
        // =========================================================

        private void CheckForRepCompletion()
        {
            if (!validationTester.IsRepCompleted)
                return;


            // =====================================================
            // MANAGER COUNTS THE REP
            // =====================================================

            currentRep++;

            totalReps++;


            if (showDebugLogs)
            {
                Debug.Log(
                    "[ExerciseManager] REP COMPLETED");

                Debug.Log(
                    $"[ExerciseManager] " +
                    $"Set {currentSet}/{targetSets} | " +
                    $"Rep {currentRep}/{targetRepsPerSet}");
            }


            // =====================================================
            // CONSUME REP
            // =====================================================

            validationTester.ConsumeRep();


            // Tell listeners (e.g. the game bridge) exactly once per validated rep.
            RepCompleted?.Invoke(totalReps);


            // =====================================================
            // CHECK SET
            // =====================================================

            CheckSetCompletion();
        }


        // =========================================================
        // SET COMPLETION
        // =========================================================

        private void CheckSetCompletion()
        {
            if (currentRep <
                targetRepsPerSet)
            {
                return;
            }


            if (showDebugLogs)
            {
                Debug.Log(
                    $"[ExerciseManager] " +
                    $"SET {currentSet} COMPLETED.");
            }


            // =====================================================
            // ALL SETS COMPLETED
            // =====================================================

            if (currentSet >= targetSets)
            {
                CompleteExercise();

                return;
            }


            // =====================================================
            // NEXT SET
            // =====================================================

            StartNextSet();
        }


        // =========================================================
        // START NEXT SET
        // =========================================================

        private void StartNextSet()
        {
            currentSet++;

            currentRep = 0;


            validationTester.Reset();


            if (showDebugLogs)
            {
                Debug.Log(
                    "[ExerciseManager] =============================");

                Debug.Log(
                    "[ExerciseManager] NEXT SET STARTED");

                Debug.Log(
                    $"[ExerciseManager] " +
                    $"Set: {currentSet}/{targetSets}");

                Debug.Log(
                    $"[ExerciseManager] " +
                    $"Rep: {currentRep}/{targetRepsPerSet}");

                Debug.Log(
                    "[ExerciseManager] =============================");
            }
        }


        // =========================================================
        // COMPLETE EXERCISE
        // =========================================================

        private void CompleteExercise()
        {
            exerciseCompleted = true;

            isRunning = false;

            isPaused = false;


            if (showDebugLogs)
            {
                Debug.Log(
                    "[ExerciseManager] =============================");

                Debug.Log(
                    "[ExerciseManager] EXERCISE COMPLETED");

                Debug.Log(
                    $"[ExerciseManager] " +
                    $"Sets: {targetSets}/{targetSets}");

                Debug.Log(
                    $"[ExerciseManager] " +
                    $"Reps Per Set: {targetRepsPerSet}");

                Debug.Log(
                    "[ExerciseManager] =============================");
            }
        }


        // =========================================================
        // TARGET CONFIGURATION
        // =========================================================

        public void SetTargetReps(int reps)
        {
            targetRepsPerSet =
                Mathf.Max(1, reps);
        }


        public void SetTargetSets(int sets)
        {
            targetSets =
                Mathf.Max(1, sets);
        }


        public void SetExerciseTarget(
            int reps,
            int sets)
        {
            targetRepsPerSet =
                Mathf.Max(1, reps);

            targetSets =
                Mathf.Max(1, sets);
        }


        // =========================================================
        // RESET
        // =========================================================

        public void ResetExercise()
        {
            currentRep = 0;

            currentSet = 1;

            totalReps = 0;

            isRunning = false;

            isPaused = false;

            exerciseCompleted = false;


            if (validationTester != null)
            {
                validationTester.Reset();
            }


            if (showDebugLogs)
            {
                Debug.Log(
                    "[ExerciseManager] Exercise reset.");
            }
        }


        // =========================================================
        // REP PROGRESS
        // =========================================================

        public float GetRepProgress()
        {
            if (targetRepsPerSet <= 0)
                return 0f;


            return Mathf.Clamp01(
                (float)currentRep /
                targetRepsPerSet);
        }


        // =========================================================
        // SET PROGRESS
        // =========================================================

        public float GetSetProgress()
        {
            if (targetSets <= 0)
                return 0f;


            return Mathf.Clamp01(
                (float)(currentSet - 1) /
                targetSets);
        }


        // =========================================================
        // OVERALL PROGRESS
        // =========================================================

        public float GetOverallProgress()
        {
            if (targetSets <= 0 ||
                targetRepsPerSet <= 0)
            {
                return 0f;
            }


            int totalReps =
                targetSets *
                targetRepsPerSet;


            int completedReps =
                ((currentSet - 1) *
                 targetRepsPerSet) +
                currentRep;


            return Mathf.Clamp01(
                (float)completedReps /
                totalReps);
        }
    }
}