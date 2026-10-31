using System.Collections.Generic;
using CampfireAdventure.Core;
using UnityEngine;

namespace CampfireAdventure.Objectives
{
    /// <summary>
    /// One step of the adventure, authored as an asset. Subclasses decide when the step is complete
    /// (resources collected, an action performed, the fire lit). The objective also lists which actions it
    /// unlocks, so the order of the adventure is data rather than code.
    /// </summary>
    public abstract class ObjectiveDefinition : ScriptableObject
    {
        [SerializeField] private string id = "objective";
        [SerializeField, Tooltip("Short name used in the tracker. May contain {0} where a count is inserted.")] private string title = "Objective";
        [SerializeField, TextArea, Tooltip("One-line instruction shown under the title.")] private string instruction = "";
        [SerializeField, Tooltip("Toast shown when the objective is completed.")] private string completionMessage = "Objective complete!";
        [SerializeField, Tooltip("Shown when the player tries something that is still locked during this objective.")] private string blockedHint = "Finish the current objective first.";
        [SerializeField, Tooltip("Actions that become available when this objective starts (and stay available).")] private GameActionType[] unlocksActions = new GameActionType[0];

        public string Id => id;
        public string CompletionMessage => completionMessage;
        public string BlockedHint => blockedHint;
        public IReadOnlyList<GameActionType> UnlocksActions => unlocksActions;

        protected string RawTitle => title;
        protected string RawInstruction => instruction;

        public virtual string GetTitle(ObjectiveContext context) => title;
        public virtual string GetInstruction(ObjectiveContext context) => instruction;

        /// <summary>Optional short progress text, e.g. "1/2". Empty when there is nothing to count.</summary>
        public virtual string GetProgress(ObjectiveContext context) => "";

        public abstract bool IsComplete(ObjectiveContext context);

        public bool Unlocks(GameActionType action)
        {
            for (int i = 0; i < unlocksActions.Length; i++) if (unlocksActions[i] == action) return true;
            return false;
        }
    }
}
