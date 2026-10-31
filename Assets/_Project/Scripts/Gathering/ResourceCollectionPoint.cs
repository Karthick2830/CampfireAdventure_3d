using System;
using UnityEngine;

namespace CampfireAdventure.Gathering
{
    /// <summary>
    /// One place resources can be collected from (a log pile, a stone pile). Holds how many units remain
    /// and where the explorer should stand. A unit can be taken exactly once; an empty point stays empty
    /// until <see cref="ResetPoint"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ResourceCollectionPoint : MonoBehaviour
    {
        [SerializeField] private ResourceDefinition resource;
        [SerializeField, Min(0), Tooltip("Units available to collect here. Each collection takes one.")] private int unitsAvailable = 1;
        [SerializeField, Tooltip("Marker the explorer walks to.")] private Transform interactionPoint;
        [SerializeField, Tooltip("What the explorer faces while collecting.")] private Transform lookTarget;
        [SerializeField, Tooltip("Visual pile (optional): shows effects and hides itself when the point is empty.")] private ResourcePile pile;

        private int startUnits;

        public ResourceDefinition Resource => resource;
        public ResourceType Type => resource.Type;
        public int UnitsRemaining { get; private set; }
        public bool HasUnits => UnitsRemaining > 0;
        public Transform InteractionPoint => interactionPoint;
        public Transform LookTarget => lookTarget != null ? lookTarget : transform;
        public ResourcePile Pile => pile;

        public event Action Changed;

        private void Awake()
        {
            startUnits = unitsAvailable;
            UnitsRemaining = unitsAvailable;
        }

        /// <summary>Takes one unit. Returns the amount gained (0 if the point is empty).</summary>
        public int Take()
        {
            if (UnitsRemaining <= 0) return 0;
            UnitsRemaining--;
            if (pile != null) pile.TakeAll();
            Changed?.Invoke();
            return resource.AmountPerCollection;
        }

        public void ResetPoint()
        {
            UnitsRemaining = startUnits;
            if (pile != null) pile.ResetPile();
            Changed?.Invoke();
        }
    }
}
