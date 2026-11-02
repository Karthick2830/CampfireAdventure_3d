using System;
using System.Collections.Generic;
using CampfireAdventure.Core;
using ExerciseGame.Core.Exercise;
using UnityEngine;

namespace CampfireAdventure.GameInput
{
    /// <summary>
    /// Exercise input source: every repetition confirmed by the exercise validator requests the next
    /// action of a fixed sequence, instead of a key press. It only raises <see cref="ActionRequested"/>;
    /// <see cref="GameActionController"/> still decides whether and how the action runs.
    /// It is enabled only while the game is playing (see GameFlowController), so reps made on the
    /// title screen or while paused are not turned into actions.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ExerciseActionSource : MonoBehaviour, IGameActionSource
    {
        [SerializeField] private ExerciseManager exercise;
        [SerializeField, Tooltip("Restarting the adventure also restarts the exercise and the sequence.")] private AdventureSession session;

        [SerializeField, Tooltip("The action requested by rep 1, rep 2, ... in order.")]
        private List<GameActionType> sequence = new List<GameActionType>
        {
            GameActionType.GoToLumberyard,   // rep 1
            GameActionType.CollectWood,      // rep 2  (first of two wood collections)
            GameActionType.CollectWood,      // rep 3
            GameActionType.GoToQuarry,       // rep 4
            GameActionType.CollectStones,    // rep 5  (first of two stone collections)
            GameActionType.CollectStones,    // rep 6
            GameActionType.ReturnToCampsite, // rep 7
            GameActionType.PrepareCampfire,  // rep 8
            GameActionType.IgniteCampfire,   // rep 9  (first attempt: sparks only)
            GameActionType.IgniteCampfire,   // rep 10 (second attempt: the fire lights)
        };

        [SerializeField] private bool logRequests = true;

        [SerializeField, Tooltip("Used to wait until the explorer is free and to check an action is allowed before it is spent.")]
        private GameActionController controller;

        private int stepIndex;
        private int pendingReps;
        private bool refusalLogged;

        /// <summary>Validated reps that are still waiting for the explorer to finish the previous action.</summary>
        public int PendingReps => pendingReps;

        public event Action<GameActionType> ActionRequested;

        /// <summary>How many steps of the sequence have been requested so far.</summary>
        public int StepIndex => stepIndex;

        public int StepCount => sequence.Count;

        /// <summary>The action the next rep will request, or false when the sequence is finished.</summary>
        public bool TryPeekNext(out GameActionType next)
        {
            bool has = stepIndex < sequence.Count;
            next = has ? sequence[stepIndex] : default;
            return has;
        }

        private void OnEnable()
        {
            if (exercise != null) exercise.RepCompleted += OnRepCompleted;
            if (session != null) session.Restarted += OnSessionRestarted;
        }

        private void OnDisable()
        {
            if (exercise != null) exercise.RepCompleted -= OnRepCompleted;
            if (session != null) session.Restarted -= OnSessionRestarted;
        }

        private void OnRepCompleted(int totalReps)
        {
            if (stepIndex + pendingReps >= sequence.Count)
            {
                if (logRequests) Debug.Log($"[ExerciseActionSource] Rep {totalReps}: the sequence is already finished, ignored.", this);
                return;
            }

            pendingReps++; // the rep is banked; it is spent as soon as the explorer is free
            if (logRequests) Debug.Log($"[ExerciseActionSource] Rep {totalReps} banked ({pendingReps} waiting).", this);
        }

        private void Update()
        {
            if (pendingReps <= 0 || !TryPeekNext(out GameActionType action)) return;
            if (controller != null && controller.IsBusy) return; // one rep = one action; wait for the previous one to end

            if (controller != null && !controller.Validate(action, out string reason))
            {
                if (logRequests && !refusalLogged) Debug.Log($"[ExerciseActionSource] {action} is not allowed yet ({reason}); the rep is kept.", this);
                refusalLogged = true;
                return;
            }

            refusalLogged = false;
            pendingReps--;
            stepIndex++;
            if (logRequests) Debug.Log($"[ExerciseActionSource] -> {action} (step {stepIndex}/{sequence.Count})", this);
            ActionRequested?.Invoke(action);
        }

        private void OnSessionRestarted()
        {
            stepIndex = 0;
            pendingReps = 0;
            refusalLogged = false;
            if (exercise == null) return;
            exercise.ResetExercise();
            exercise.StartExercise();
        }

        /// <summary>Pretend a rep was validated (testing without a camera).</summary>
        [ContextMenu("Simulate Rep")]
        public void SimulateRep() => OnRepCompleted(stepIndex + pendingReps + 1);
    }
}
