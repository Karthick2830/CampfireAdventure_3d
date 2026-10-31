using System;
using System.Collections;
using System.Collections.Generic;
using CampfireAdventure.Player;
using UnityEngine;

namespace CampfireAdventure.Core
{
    /// <summary>Designer-facing description of one gameplay action: where to go and what to play there.</summary>
    [Serializable]
    public sealed class ActionBinding
    {
        public GameActionType action;
        [Tooltip("Short name shown in the HUD.")]
        public string displayName;
        [Tooltip("Interaction point the explorer walks to (stand-here marker).")]
        public Transform destination;
        [Tooltip("Object the explorer turns to face on arrival.")]
        public Transform lookTarget;
        public PlayerActionAnimation animation = PlayerActionAnimation.None;
        [Min(0f), Tooltip("Total seconds the explorer spends performing the action after arriving.")]
        public float performDuration = 1.9f;
        [Min(0f), Tooltip("Seconds into the performance when ActionPerformed fires (the moment the effect should apply).")]
        public float effectTime = 0.9f;
        [Min(0f), Tooltip("If the explorer is already this close to the destination, skip walking and perform immediately.")]
        public float skipNavigationWithin = 0.75f;
    }

    /// <summary>
    /// Receives named action requests from any <see cref="IGameActionSource"/>, validates them and
    /// runs them through Idle -> Navigating -> PerformingAction -> Completing -> Idle (or Failed).
    /// It does not read input, change inventory or touch UI; it only raises events for those systems.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameActionController : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField, Tooltip("Components implementing IGameActionSource (e.g. KeyboardActionSource).")]
        private List<MonoBehaviour> actionSources = new List<MonoBehaviour>();

        [SerializeField, Tooltip("Optional components implementing IActionValidator.")]
        private List<MonoBehaviour> validators = new List<MonoBehaviour>();

        [SerializeField, Tooltip("Optional components implementing IActionTargetProvider (dynamic destinations).")]
        private List<MonoBehaviour> targetProviders = new List<MonoBehaviour>();

        [SerializeField] private AutoNavigationController navigation;
        [SerializeField] private PlayerAnimationDriver animationDriver;

        [Header("Actions")]
        [SerializeField] private List<ActionBinding> bindings = new List<ActionBinding>();

        [Header("Behaviour")]
        [SerializeField, Tooltip("What to do with a request that arrives while an action is running.")]
        private BusyPolicy busyPolicy = BusyPolicy.Reject;

        [SerializeField, Min(0f), Tooltip("Pause after performing, before returning to idle.")]
        private float completeDelay = 0.2f;

        [SerializeField, Min(0f), Tooltip("How long the Failed state is held so feedback can be shown.")]
        private float failedHoldSeconds = 0.6f;

        private readonly Dictionary<GameActionType, ActionBinding> map = new Dictionary<GameActionType, ActionBinding>();
        private readonly List<IActionValidator> validatorCache = new List<IActionValidator>();
        private readonly List<IActionTargetProvider> providerCache = new List<IActionTargetProvider>();
        private Coroutine running;
        private bool runActive; // set the instant an action is accepted, cleared when it ends
        private GameActionType? queued;
        private Action pendingArrived;
        private Action<string> pendingFailed;

        public ActionState State { get; private set; } = ActionState.Idle;
        public GameActionType? CurrentAction { get; private set; }
        /// <summary>True from the moment an action is accepted (even in the frame before the state changes) until it ends.</summary>
        public bool IsBusy => State != ActionState.Idle || runActive;

        public event Action<ActionState> StateChanged;
        public event Action<GameActionType> ActionStarted;
        public event Action<GameActionType> ActionArrived;
        /// <summary>Fires exactly once per successful action at <see cref="ActionBinding.effectTime"/>; apply gameplay effects here.</summary>
        public event Action<GameActionType> ActionPerformed;
        public event Action<GameActionType> ActionCompleted;
        public event Action<GameActionType, string> ActionRejected;
        public event Action<GameActionType, string> ActionFailed;

        public string GetDisplayName(GameActionType action)
        {
            return map.TryGetValue(action, out var b) && !string.IsNullOrEmpty(b.displayName) ? b.displayName : action.ToString();
        }

        private void Awake()
        {
            foreach (var b in bindings) if (b != null) map[b.action] = b;

            foreach (var mb in targetProviders)
            {
                if (mb is IActionTargetProvider p) providerCache.Add(p);
                else if (mb != null) Debug.LogError($"{mb.name} does not implement IActionTargetProvider.", this);
            }

            foreach (var mb in validators)
            {
                if (mb is IActionValidator v) validatorCache.Add(v);
                else if (mb != null) Debug.LogError($"{mb.name} does not implement IActionValidator.", this);
            }
        }

        private void OnEnable()
        {
            foreach (var mb in actionSources)
            {
                if (mb is IGameActionSource source) source.ActionRequested += OnActionRequested;
                else if (mb != null) Debug.LogError($"{mb.name} does not implement IGameActionSource.", this);
            }
        }

        private void OnDisable()
        {
            foreach (var mb in actionSources)
                if (mb is IGameActionSource source) source.ActionRequested -= OnActionRequested;
            Cancel();
        }

        private void OnActionRequested(GameActionType action) => TryRequest(action, out _);

        /// <summary>Requests an action. Returns true if it started (or was queued); otherwise reports why.</summary>
        public bool TryRequest(GameActionType action, out string message)
        {
            message = null;
            if (!map.TryGetValue(action, out var binding) || binding.destination == null)
            {
                message = $"{action} has no destination configured.";
                ActionRejected?.Invoke(action, message);
                return false;
            }

            if (IsBusy)
            {
                if (busyPolicy == BusyPolicy.QueueLatest)
                {
                    queued = action;
                    message = "Queued: " + binding.displayName;
                    return true;
                }
                message = "The explorer is busy right now.";
                ActionRejected?.Invoke(action, message);
                return false;
            }

            if (!Validate(action, out message))
            {
                ActionRejected?.Invoke(action, message);
                return false;
            }

            runActive = true;
            var routine = StartCoroutine(Run(action, binding));
            if (runActive) running = routine; // not already finished synchronously
            return true;
        }

        /// <summary>
        /// Checks whether an action would be allowed right now (destination configured and every
        /// validator agrees) without starting it or raising events. Ignores whether the explorer is busy.
        /// </summary>
        public bool Validate(GameActionType action, out string reason)
        {
            reason = null;
            if (!map.TryGetValue(action, out var binding) || binding.destination == null)
            {
                reason = $"{action} has no destination configured.";
                return false;
            }
            for (int i = 0; i < validatorCache.Count; i++)
            {
                if (!validatorCache[i].CanStart(action, out string why))
                {
                    reason = string.IsNullOrEmpty(why) ? "That action is not available yet." : why;
                    return false;
                }
            }
            return true;
        }

        /// <summary>Stops the current action and clears the queue. Inventory/objective state is untouched.</summary>
        public void Cancel()
        {
            if (running != null) StopCoroutine(running);
            running = null;
            runActive = false;
            DetachNavigationHandlers();
            queued = null;
            if (navigation != null) navigation.Cancel();
            if (animationDriver != null) animationDriver.ResetTriggers();
            CurrentAction = null;
            SetState(ActionState.Idle);
        }

        private IEnumerator Run(GameActionType action, ActionBinding binding)
        {
            CurrentAction = action;
            ActionStarted?.Invoke(action);

            Transform destinationT = binding.destination, lookT = binding.lookTarget;
            for (int i = 0; i < providerCache.Count; i++)
            {
                if (providerCache[i].TryResolve(action, navigation.transform.position, out Transform d, out Transform l))
                {
                    destinationT = d; lookT = l;
                    break;
                }
            }
            Vector3 destination = destinationT.position;
            if (navigation.DistanceTo(destination) > binding.skipNavigationWithin)
            {
                SetState(ActionState.Navigating);

                bool done = false, success = true;
                string failure = null;
                pendingArrived = () => done = true;
                pendingFailed = reason => { success = false; failure = reason; done = true; };
                navigation.Arrived += pendingArrived;
                navigation.Failed += pendingFailed;

                if (!navigation.TryStartNavigation(destination, out string startFailure))
                {
                    success = false; failure = startFailure; done = true;
                }
                while (!done) yield return null;
                DetachNavigationHandlers();

                if (!success)
                {
                    yield return Fail(action, failure);
                    yield break;
                }
            }

            ActionArrived?.Invoke(action);
            yield return navigation.FaceTarget(lookT);

            SetState(ActionState.PerformingAction);
            animationDriver.Play(binding.animation);
            float effectAt = Mathf.Min(binding.effectTime, binding.performDuration);
            if (effectAt > 0f) yield return new WaitForSeconds(effectAt);
            ActionPerformed?.Invoke(action);
            float rest = binding.performDuration - effectAt;
            if (rest > 0f) yield return new WaitForSeconds(rest);

            SetState(ActionState.Completing);
            if (completeDelay > 0f) yield return new WaitForSeconds(completeDelay);
            ActionCompleted?.Invoke(action);
            Finish();
        }

        private IEnumerator Fail(GameActionType action, string reason)
        {
            SetState(ActionState.Failed);
            ActionFailed?.Invoke(action, reason);
            if (failedHoldSeconds > 0f) yield return new WaitForSeconds(failedHoldSeconds);
            Finish();
        }

        private void Finish()
        {
            running = null;
            runActive = false;
            CurrentAction = null;
            SetState(ActionState.Idle);
            if (queued.HasValue)
            {
                var next = queued.Value;
                queued = null;
                TryRequest(next, out _);
            }
        }

        private void DetachNavigationHandlers()
        {
            if (navigation == null) return;
            if (pendingArrived != null) navigation.Arrived -= pendingArrived;
            if (pendingFailed != null) navigation.Failed -= pendingFailed;
            pendingArrived = null;
            pendingFailed = null;
        }

        private void SetState(ActionState state)
        {
            if (State == state) return;
            State = state;
            StateChanged?.Invoke(state);
        }
    }
}
