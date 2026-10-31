using CampfireAdventure.Core;
using UnityEngine;

namespace CampfireAdventure.Objectives
{
    /// <summary>Complete once a given gameplay action has been performed while this objective is active.</summary>
    [CreateAssetMenu(fileName = "Objective_", menuName = "Campfire Adventure/Objective/Perform Action")]
    public sealed class ActionObjective : ObjectiveDefinition
    {
        [SerializeField] private GameActionType action = GameActionType.ReturnToCampsite;

        public override bool IsComplete(ObjectiveContext c) => c.WasPerformed(action);
    }
}
