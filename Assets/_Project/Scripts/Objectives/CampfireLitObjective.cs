using CampfireAdventure.Campfire;
using UnityEngine;

namespace CampfireAdventure.Objectives
{
    /// <summary>Complete once the campfire is burning.</summary>
    [CreateAssetMenu(fileName = "Objective_", menuName = "Campfire Adventure/Objective/Campfire Lit")]
    public sealed class CampfireLitObjective : ObjectiveDefinition
    {
        public override bool IsComplete(ObjectiveContext c) => c.Campfire.State == CampfireState.Lit;
    }
}
