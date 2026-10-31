using System;
using CampfireAdventure.Core;
using UnityEngine;

namespace CampfireAdventure.Interaction
{
    /// <summary>
    /// Something the explorer can interact with when standing next to it (a resource pile, the
    /// campfire). Interacting never does the work itself: it requests <see cref="Action"/> from the
    /// action controller, so the same validation, one-at-a-time rule and effects apply as for keys 1-7.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Text for the prompt, e.g. "Chop wood (1/2)". Must not allocate on every read.</summary>
        string Prompt { get; }

        /// <summary>The gameplay action this object currently offers.</summary>
        GameActionType Action { get; }

        /// <summary>False when the object offers nothing right now (e.g. the fire is already burning).</summary>
        bool IsVisible { get; }

        /// <summary>World position used to decide which nearby interactable is closest.</summary>
        Vector3 Position { get; }

        /// <summary>True when the action could start now; otherwise a short player-facing reason.</summary>
        bool CanInteract(out string reason);

        /// <summary>Raised when the prompt, visibility or availability may have changed.</summary>
        event Action Changed;
    }
}
