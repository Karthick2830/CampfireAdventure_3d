namespace CampfireAdventure.Core
{
    /// <summary>
    /// Named gameplay actions. Input sources (keyboard now, the exercise validator later)
    /// only ever request one of these; they never move the character directly.
    /// </summary>
    public enum GameActionType
    {
        GoToLumberyard = 1,
        CollectWood = 2,
        GoToQuarry = 3,
        CollectStones = 4,
        ReturnToCampsite = 5,
        PrepareCampfire = 6,
        IgniteCampfire = 7,
    }

    /// <summary>High level phase of the action state machine.</summary>
    public enum ActionState
    {
        Idle,
        Navigating,
        PerformingAction,
        Completing,
        Failed,
    }

    /// <summary>What the controller does with a request that arrives while busy.</summary>
    public enum BusyPolicy
    {
        /// <summary>Ignore the request and report why.</summary>
        Reject,
        /// <summary>Keep only the most recent request and run it when the current action ends.</summary>
        QueueLatest,
    }

    /// <summary>Character animations that can be played as a gameplay action.</summary>
    public enum PlayerActionAnimation
    {
        None,
        Pickup,
        Victory,
        Wave,
        /// <summary>Overhead axe swings (procedural pose on the humanoid rig, axe prop in hand).</summary>
        AxeChop,
    }
}
