using System.Collections.Generic;
using CampfireAdventure.Campfire;
using CampfireAdventure.Core;
using CampfireAdventure.Gathering;

namespace CampfireAdventure.Objectives
{
    /// <summary>
    /// What an objective is allowed to look at when deciding whether it is complete: the inventory, the
    /// campfire, and which actions have been performed since the objective began.
    /// </summary>
    public sealed class ObjectiveContext
    {
        private readonly HashSet<GameActionType> performed = new HashSet<GameActionType>();

        public ResourceInventory Inventory { get; }
        public CampfireController Campfire { get; }

        public ObjectiveContext(ResourceInventory inventory, CampfireController campfire)
        {
            Inventory = inventory;
            Campfire = campfire;
        }

        public void NotePerformed(GameActionType action) => performed.Add(action);
        public bool WasPerformed(GameActionType action) => performed.Contains(action);
        public void ClearPerformed() => performed.Clear();
    }
}
