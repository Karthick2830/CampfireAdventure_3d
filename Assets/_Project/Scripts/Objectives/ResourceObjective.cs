using CampfireAdventure.Gathering;
using UnityEngine;

namespace CampfireAdventure.Objectives
{
    /// <summary>Complete once the inventory holds the amount of a resource the campfire needs.</summary>
    [CreateAssetMenu(fileName = "Objective_", menuName = "Campfire Adventure/Objective/Collect Resource")]
    public sealed class ResourceObjective : ObjectiveDefinition
    {
        [SerializeField] private ResourceType resource = ResourceType.Wood;

        public override string GetTitle(ObjectiveContext c) => string.Format(RawTitle, c.Inventory.Required(resource));
        public override string GetInstruction(ObjectiveContext c) => string.Format(RawInstruction, c.Inventory.Required(resource));
        public override string GetProgress(ObjectiveContext c) => c.Inventory.Get(resource) + "/" + c.Inventory.Required(resource);
        public override bool IsComplete(ObjectiveContext c) => c.Inventory.IsFull(resource);
    }
}
