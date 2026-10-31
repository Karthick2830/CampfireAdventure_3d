using System;
using UnityEngine;

namespace CampfireAdventure.Gathering
{
    public enum ResourceType
    {
        Wood,
        Stone,
    }

    /// <summary>
    /// Wood and stone counts, kept independent, with the amounts the campfire needs (taken from the
    /// resource definitions). Raises <see cref="Changed"/> whenever a count changes.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ResourceInventory : MonoBehaviour
    {
        [SerializeField] private ResourceDefinition wood;
        [SerializeField] private ResourceDefinition stone;

        public int Wood { get; private set; }
        public int Stones { get; private set; }
        public int RequiredWood => wood.RequiredForCampfire;
        public int RequiredStones => stone.RequiredForCampfire;

        /// <summary>Total collected since the adventure began (not reduced when resources are placed at the fire).</summary>
        public int TotalWoodCollected { get; private set; }
        public int TotalStonesCollected { get; private set; }

        public event Action Changed;

        public ResourceDefinition Definition(ResourceType type) => type == ResourceType.Wood ? wood : stone;
        public int Get(ResourceType type) => type == ResourceType.Wood ? Wood : Stones;
        public int Required(ResourceType type) => Definition(type).RequiredForCampfire;
        public bool IsFull(ResourceType type) => Get(type) >= Required(type);
        public bool HasEnoughForCampfire => Wood >= RequiredWood && Stones >= RequiredStones;

        /// <summary>Adds up to the amount the campfire needs; extra is discarded. Returns how many were actually added.</summary>
        public int Add(ResourceType type, int amount)
        {
            int before = Get(type);
            int after = Mathf.Min(Required(type), before + Mathf.Max(0, amount));
            if (type == ResourceType.Wood) { Wood = after; TotalWoodCollected += after - before; }
            else { Stones = after; TotalStonesCollected += after - before; }
            if (after != before) Changed?.Invoke();
            return after - before;
        }

        /// <summary>Uses up the amounts the campfire needs. Returns false (and changes nothing) if there is not enough.</summary>
        public bool TrySpendForCampfire()
        {
            if (!HasEnoughForCampfire) return false;
            Wood -= RequiredWood;
            Stones -= RequiredStones;
            Changed?.Invoke();
            return true;
        }

        public void Clear()
        {
            Wood = 0; Stones = 0;
            TotalWoodCollected = 0; TotalStonesCollected = 0;
            Changed?.Invoke();
        }
    }
}
