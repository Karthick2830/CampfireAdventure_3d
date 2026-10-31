using System;
using System.Collections.Generic;
using CampfireAdventure.Core;
using UnityEngine;

namespace CampfireAdventure.Interaction
{
    /// <summary>
    /// Finds the nearest visible <see cref="IInteractable"/> around the explorer and, when the contextual
    /// interact key is pressed, asks the action controller to perform its action. Detection is a
    /// non-allocating physics overlap on the Interactable layer, so interactables only need a collider.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private GameActionController actions;
        [SerializeField, Tooltip("Components implementing IInteractInputSource (e.g. KeyboardActionSource).")]
        private List<MonoBehaviour> inputSources = new List<MonoBehaviour>();
        [SerializeField, Tooltip("The explorer; detection is centred here.")]
        private Transform origin;

        [Header("Detection")]
        [SerializeField, Min(0.5f), Tooltip("How close (world units) an interactable's collider must be.")]
        private float range = 3.5f;
        [SerializeField] private LayerMask interactableLayers;
        [SerializeField, Min(0.02f), Tooltip("Seconds between detection queries.")]
        private float scanInterval = 0.1f;

        private readonly Collider[] hits = new Collider[16];
        private IInteractable current;
        private float nextScan;

        /// <summary>The nearest visible interactable in range, or null.</summary>
        public IInteractable Current => current;

        /// <summary>Raised when <see cref="Current"/> changes (including to null).</summary>
        public event Action<IInteractable> CurrentChanged;

        /// <summary>Raised when the current interactable reports a change in prompt or availability.</summary>
        public event Action CurrentUpdated;

        /// <summary>Short player-facing messages, e.g. when nothing is in range.</summary>
        public event Action<string> Notice;

        private void OnEnable()
        {
            foreach (var mb in inputSources)
            {
                if (mb is IInteractInputSource s) s.InteractRequested += OnInteractRequested;
                else if (mb != null) Debug.LogError($"{mb.name} does not implement IInteractInputSource.", this);
            }
        }

        private void OnDisable()
        {
            foreach (var mb in inputSources)
                if (mb is IInteractInputSource s) s.InteractRequested -= OnInteractRequested;
            SetCurrent(null);
        }

        private void OnInteractRequested() => TryInteract();

        private void Update()
        {
            if (Time.unscaledTime < nextScan) return;
            nextScan = Time.unscaledTime + scanInterval;
            SetCurrent(FindNearest());
        }

        /// <summary>
        /// Requests the current interactable's action. Returns false when nothing is in range, or when the
        /// action controller refuses (busy, or a validator says no); the refusal reason is raised by the
        /// controller's ActionRejected event so every refusal reaches the player the same way.
        /// </summary>
        public bool TryInteract()
        {
            if (current == null)
            {
                Notice?.Invoke("There is nothing to interact with here.");
                return false;
            }
            if (!current.CanInteract(out string reason))
            {
                Notice?.Invoke(reason); // e.g. "This pile is empty." / "You already have all the wood you need"
                return false;
            }
            return actions.TryRequest(current.Action, out _);
        }

        private IInteractable FindNearest()
        {
            Vector3 center = origin.position;
            int count = Physics.OverlapSphereNonAlloc(center, range, hits, interactableLayers, QueryTriggerInteraction.Collide);

            IInteractable best = null;
            float bestSqr = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                var candidate = hits[i].GetComponentInParent<IInteractable>();
                if (candidate == null || !candidate.IsVisible) continue;

                Vector3 d = candidate.Position - center; d.y = 0f;
                float sqr = d.sqrMagnitude;
                if (sqr < bestSqr) { bestSqr = sqr; best = candidate; }
            }
            return best;
        }

        private void SetCurrent(IInteractable next)
        {
            if (ReferenceEquals(next, current)) return;
            if (current != null) current.Changed -= OnCurrentChanged;
            current = next;
            if (current != null) current.Changed += OnCurrentChanged;
            CurrentChanged?.Invoke(current);
        }

        private void OnCurrentChanged()
        {
            // The object's own visibility may have flipped (e.g. the fire is now lit): re-evaluate right away.
            if (current != null && !current.IsVisible) SetCurrent(FindNearest());
            else CurrentUpdated?.Invoke();
        }
    }
}
