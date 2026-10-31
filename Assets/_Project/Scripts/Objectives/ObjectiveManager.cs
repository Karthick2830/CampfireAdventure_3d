using System;
using System.Collections.Generic;
using CampfireAdventure.Campfire;
using CampfireAdventure.Core;
using CampfireAdventure.Gathering;
using UnityEngine;

namespace CampfireAdventure.Objectives
{
    /// <summary>
    /// Runs the ordered list of objectives. It completes each one when its definition says so, unlocks the
    /// next, and (as an <see cref="IActionValidator"/>) refuses actions the adventure has not unlocked yet,
    /// with the current objective's hint as the reason. All story order lives in the objective assets.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ObjectiveManager : MonoBehaviour, IActionValidator
    {
        [SerializeField] private GameActionController actions;
        [SerializeField] private ResourceInventory inventory;
        [SerializeField] private CampfireController campfire;
        [SerializeField, Tooltip("In order. The adventure is complete when the last one is.")]
        private List<ObjectiveDefinition> objectives = new List<ObjectiveDefinition>();
        [SerializeField, Tooltip("Shown when the player acts after the adventure is finished.")]
        private string finishedMessage = "The adventure is complete! Press F5 to play again.";

        private ObjectiveContext context;

        public int Count => objectives.Count;
        public int Index { get; private set; }
        public bool IsFinished => Index >= objectives.Count;
        public ObjectiveDefinition Current => IsFinished ? null : objectives[Index];
        public ObjectiveContext Context => context;

        /// <summary>Raised after any change to the objective state (use to refresh the tracker).</summary>
        public event Action Changed;
        public event Action<ObjectiveDefinition, int> ObjectiveStarted;
        public event Action<ObjectiveDefinition, int> ObjectiveCompleted;
        public event Action AllCompleted;

        public ObjectiveDefinition Get(int i) => objectives[i];
        public bool IsDone(int i) => i < Index;
        public string TitleOf(int i) => objectives[i].GetTitle(context);
        public string ProgressOf(int i) => objectives[i].GetProgress(context);

        private void Awake() => context = new ObjectiveContext(inventory, campfire);

        private void OnEnable()
        {
            actions.ActionPerformed += OnPerformed;
            inventory.Changed += Evaluate;
            campfire.StateChanged += OnFireChanged;
        }

        private void OnDisable()
        {
            actions.ActionPerformed -= OnPerformed;
            inventory.Changed -= Evaluate;
            campfire.StateChanged -= OnFireChanged;
        }

        private void Start() => BeginFromStart();

        /// <summary>Starts the adventure again from the first objective.</summary>
        public void ResetAll()
        {
            BeginFromStart();
        }

        public bool CanStart(GameActionType action, out string reason)
        {
            reason = null;
            if (IsFinished) { reason = finishedMessage; return false; }
            for (int i = 0; i <= Index; i++)
                if (objectives[i].Unlocks(action)) return true;
            reason = objectives[Index].BlockedHint;
            return false;
        }

        private void BeginFromStart()
        {
            Index = 0;
            context.ClearPerformed();
            if (!IsFinished) ObjectiveStarted?.Invoke(Current, Index);
            Changed?.Invoke();
            Evaluate(); // an objective may already be satisfied
        }

        private void OnPerformed(GameActionType action)
        {
            context.NotePerformed(action);
            Evaluate();
        }

        private void OnFireChanged(CampfireState _) => Evaluate();

        private void Evaluate()
        {
            bool changed = false;
            while (!IsFinished && Current.IsComplete(context))
            {
                var done = Current; int doneIndex = Index;
                Index++;
                context.ClearPerformed();
                changed = true;
                ObjectiveCompleted?.Invoke(done, doneIndex);
                if (IsFinished) AllCompleted?.Invoke();
                else ObjectiveStarted?.Invoke(Current, Index);
            }
            // progress text (e.g. 1/2) also changes without completing anything
            Changed?.Invoke();
            if (changed) return;
        }
    }
}
