using System;
using CampfireAdventure.Core;
using CampfireAdventure.Gathering;
using UnityEngine;

namespace CampfireAdventure.Interaction
{
    /// <summary>A wood or stone collection point. Offers the matching collect action with live counts.</summary>
    [DisallowMultipleComponent]
    public sealed class ResourcePointInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private ResourceCollectionPoint point;
        [SerializeField] private GameActionController actions;
        [SerializeField] private ResourceInventory inventory;

        private string prompt = "";

        public event Action Changed;

        public string Prompt => prompt;
        public GameActionType Action => point.Type == ResourceType.Wood ? GameActionType.CollectWood : GameActionType.CollectStones;
        public bool IsVisible => true;
        public Vector3 Position => transform.position;

        private void OnEnable()
        {
            inventory.Changed += OnChanged;
            point.Changed += OnChanged;
            actions.StateChanged += OnActionStateChanged;
            Rebuild();
        }

        private void OnDisable()
        {
            inventory.Changed -= OnChanged;
            point.Changed -= OnChanged;
            actions.StateChanged -= OnActionStateChanged;
        }

        public bool CanInteract(out string reason)
        {
            if (!point.HasUnits)
            {
                reason = point.Type == ResourceType.Wood ? "This log pile is empty." : "This stone pile is already broken up.";
                return false;
            }
            return actions.Validate(Action, out reason);
        }

        private void OnChanged()
        {
            Rebuild();
            Changed?.Invoke();
        }

        private void OnActionStateChanged(ActionState _) => Changed?.Invoke();

        private void Rebuild()
        {
            var def = point.Resource;
            prompt = def.CollectVerb + " (" + inventory.Get(point.Type) + "/" + inventory.Required(point.Type) + ")";
            if (!point.HasUnits) prompt = def.CollectVerb + " (empty)";
        }
    }
}
