using System;
using UnityEngine;

namespace CampfireAdventure.Core
{
    /// <summary>
    /// Anything that can ask the game to perform a named action: the keyboard prototype today,
    /// a validated-repetition adapter later. Sources must not touch the character or NavMeshAgent.
    /// </summary>
    public interface IGameActionSource
    {
        /// <summary>Raised when the source wants <see cref="GameActionController"/> to start an action.</summary>
        event Action<GameActionType> ActionRequested;
    }

    /// <summary>
    /// A source of the contextual "interact with whatever is in front of me" command (the E key today).
    /// Like <see cref="IGameActionSource"/>, it only raises an event; gameplay decides what it means.
    /// </summary>
    public interface IInteractInputSource
    {
        event Action InteractRequested;
    }

    /// <summary>A source of the menu "back / pause" command (Esc today). Works even while the game is paused.</summary>
    public interface IMenuInputSource
    {
        event Action MenuBackRequested;
    }

    /// <summary>A source of the "start the adventure again" command (F5 today; a pause-menu button later).</summary>
    public interface ISessionInputSource
    {
        event Action RestartRequested;
    }

    /// <summary>
    /// Optional provider of a dynamic destination for an action (e.g. "the nearest pile that still has
    /// wood"). Consulted once when the action starts; return false to use the action's configured destination.
    /// </summary>
    public interface IActionTargetProvider
    {
        bool TryResolve(GameActionType action, Vector3 from, out Transform destination, out Transform lookTarget);
    }

    /// <summary>
    /// Optional gate consulted before an action starts (objective order, missing resources, ...).
    /// Return false and a short player-facing reason to refuse the action.
    /// </summary>
    public interface IActionValidator
    {
        bool CanStart(GameActionType action, out string reason);
    }
}
