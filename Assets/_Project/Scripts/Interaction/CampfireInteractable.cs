using System;
using CampfireAdventure.Campfire;
using CampfireAdventure.Core;
using CampfireAdventure.Gathering;
using UnityEngine;

namespace CampfireAdventure.Interaction
{
    /// <summary>
    /// The fire pit. Offers "prepare" while unlit, "light" once prepared (or "try again" after a failed
    /// attempt), and nothing once the fire is burning.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CampfireInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private CampfireController campfire;
        [SerializeField] private GameActionController actions;
        [SerializeField] private ResourceInventory inventory;

        private string prompt = "";

        public event Action Changed;

        public string Prompt => prompt;
        public GameActionType Action => campfire.State == CampfireState.Unlit ? GameActionType.PrepareCampfire : GameActionType.IgniteCampfire;
        public bool IsVisible => campfire.State == CampfireState.Unlit || campfire.State == CampfireState.Prepared;
        public Vector3 Position => transform.position;

        private void OnEnable()
        {
            campfire.StateChanged += OnFireChanged;
            inventory.Changed += OnInventoryChanged;
            actions.StateChanged += OnActionStateChanged;
            Rebuild();
        }

        private void OnDisable()
        {
            campfire.StateChanged -= OnFireChanged;
            inventory.Changed -= OnInventoryChanged;
            actions.StateChanged -= OnActionStateChanged;
        }

        public bool CanInteract(out string reason) => actions.Validate(Action, out reason);

        private void OnFireChanged(CampfireState _)
        {
            Rebuild();
            Changed?.Invoke();
        }

        private void OnInventoryChanged() => Changed?.Invoke();
        private void OnActionStateChanged(ActionState state)
        {
            // a failed ignition attempt keeps the state Prepared but changes the wording
            Rebuild();
            Changed?.Invoke();
        }

        private void Rebuild()
        {
            switch (campfire.State)
            {
                case CampfireState.Unlit: prompt = "Prepare the campfire"; break;
                case CampfireState.Prepared: prompt = campfire.IgniteAttempts > 0 ? "Try lighting the campfire again" : "Light the campfire"; break;
                default: prompt = ""; break;
            }
        }
    }
}
