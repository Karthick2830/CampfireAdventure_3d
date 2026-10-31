using System;
using System.Collections;
using CampfireAdventure.Core;
using UnityEngine;

namespace CampfireAdventure.Campfire
{
    public enum CampfireState
    {
        Unlit,
        Prepared,
        Igniting,
        Lit,
    }

    /// <summary>
    /// Owns the campfire's visual states: Unlit -> Prepared (logs placed) -> Igniting (sparks) -> Lit
    /// (flames, embers, smoke, warm light). Reacts to <see cref="GameActionController"/> events
    /// and also acts as an <see cref="IActionValidator"/> so the fire cannot be lit before it is prepared.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CampfireController : MonoBehaviour, IActionValidator
    {
        [Header("Wiring")]
        [SerializeField] private GameActionController actions;

        [Header("Prepared state")]
        [SerializeField, Tooltip("Logs/kindling that appear when the campfire is prepared. Their editor pose is the final pose.")]
        private Transform[] logs;
        [SerializeField, Min(0f)] private float logDropHeight = 1.0f;
        [SerializeField, Min(0.05f)] private float logDropSeconds = 0.3f;
        [SerializeField, Min(0f)] private float logInterval = 0.14f;

        [Header("Ignition")]
        [SerializeField] private ParticleSystem ignitionSparks;
        [SerializeField, Min(1), Tooltip("Ignition attempts needed. Earlier attempts only throw sparks and ask the player to try again.")]
        private int igniteAttemptsRequired = 2;

        [Header("Lit state")]
        [SerializeField, Tooltip("Parent of flames, embers, smoke and the light; enabled when lit.")]
        private GameObject litRoot;
        [SerializeField] private FlameFlicker flames;
        [SerializeField, Min(0.05f)] private float fadeInSeconds = 0.9f;
        [SerializeField] private Renderer coals;
        [SerializeField] private Material coalsCold;
        [SerializeField] private Material coalsHot;

        private Vector3[] logPos;
        private Quaternion[] logRot;
        private Vector3[] logScale;
        private Coroutine placing, lighting;
        private GameActionType? pending;
        private int igniteAttempts;

        /// <summary>Short player-facing messages (e.g. a failed ignition attempt).</summary>
        public event Action<string> Notice;
        /// <summary>Raised each time a log lands on the pit while the campfire is being prepared.</summary>
        public event Action LogPlaced;
        public int IgniteAttempts => igniteAttempts;

        public CampfireState State { get; private set; } = CampfireState.Unlit;
        public event Action<CampfireState> StateChanged;

        private void Awake()
        {
            logPos = new Vector3[logs.Length];
            logRot = new Quaternion[logs.Length];
            logScale = new Vector3[logs.Length];
            for (int i = 0; i < logs.Length; i++)
            {
                logPos[i] = logs[i].localPosition; logRot[i] = logs[i].localRotation; logScale[i] = logs[i].localScale;
            }
            ApplyVisuals(CampfireState.Unlit);
        }

        private void OnEnable()
        {
            actions.StateChanged += OnActionStateChanged;
            actions.ActionPerformed += OnActionPerformed;
        }

        private void OnDisable()
        {
            actions.StateChanged -= OnActionStateChanged;
            actions.ActionPerformed -= OnActionPerformed;
        }

        /// <summary>Puts the fire back to its cold, unprepared state (used by restart).</summary>
        public void ResetFire()
        {
            StopRoutines();
            pending = null;
            igniteAttempts = 0;
            ApplyVisuals(CampfireState.Unlit);
            SetState(CampfireState.Unlit);
        }

        public bool CanStart(GameActionType action, out string reason)
        {
            reason = null;
            switch (action)
            {
                case GameActionType.PrepareCampfire:
                    if (State == CampfireState.Unlit) return true;
                    reason = State == CampfireState.Lit ? "The campfire is already burning." : "The campfire is already prepared.";
                    return false;
                case GameActionType.IgniteCampfire:
                    if (State == CampfireState.Prepared) return true;
                    reason = State == CampfireState.Lit ? "The campfire is already burning." : "Prepare the campfire first.";
                    return false;
                default:
                    return true;
            }
        }

        private void OnActionStateChanged(ActionState state)
        {
            if (state == ActionState.PerformingAction)
            {
                var current = actions.CurrentAction;
                if (current == GameActionType.PrepareCampfire)
                {
                    pending = current;
                    StopRoutines();
                    placing = StartCoroutine(PlaceLogs());
                }
                else if (current == GameActionType.IgniteCampfire)
                {
                    pending = current;
                    SetState(CampfireState.Igniting);
                    if (ignitionSparks != null) ignitionSparks.Play();
                }
            }
            else if ((state == ActionState.Idle || state == ActionState.Failed) && pending.HasValue)
            {
                // The action ended before its effect fired (cancelled): undo the partial visuals.
                var cancelled = pending.Value;
                pending = null;
                StopRoutines();
                if (cancelled == GameActionType.PrepareCampfire) { ApplyVisuals(CampfireState.Unlit); SetState(CampfireState.Unlit); }
                else { ApplyVisuals(CampfireState.Prepared); SetState(CampfireState.Prepared); }
            }
        }

        private void OnActionPerformed(GameActionType action)
        {
            if (action == GameActionType.PrepareCampfire)
            {
                pending = null;
                StopRoutines();
                ApplyVisuals(CampfireState.Prepared);
                SetState(CampfireState.Prepared);
            }
            else if (action == GameActionType.IgniteCampfire)
            {
                pending = null;
                igniteAttempts++;
                if (ignitionSparks != null) ignitionSparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);

                if (igniteAttempts < igniteAttemptsRequired)
                {
                    // Only sparks this time: the kindling has not caught yet.
                    SetState(CampfireState.Prepared);
                    Notice?.Invoke("Only sparks... the kindling didn't catch. Try again!");
                    return;
                }

                SetState(CampfireState.Lit);
                if (lighting != null) StopCoroutine(lighting);
                lighting = StartCoroutine(FadeInFlames());
            }
        }

        private IEnumerator PlaceLogs()
        {
            for (int i = 0; i < logs.Length; i++) logs[i].gameObject.SetActive(false);
            for (int i = 0; i < logs.Length; i++)
            {
                StartCoroutine(DropLog(i));
                yield return new WaitForSeconds(logInterval);
            }
            placing = null;
        }

        private IEnumerator DropLog(int i)
        {
            Transform t = logs[i];
            t.gameObject.SetActive(true);
            Vector3 from = logPos[i] + Vector3.up * logDropHeight;
            for (float e = 0f; e < logDropSeconds; e += Time.deltaTime)
            {
                float k = e / logDropSeconds;
                float ease = 1f - (1f - k) * (1f - k);
                t.localPosition = Vector3.Lerp(from, logPos[i], ease);
                t.localScale = logScale[i] * Mathf.Lerp(0.4f, 1f, ease);
                yield return null;
            }
            t.localPosition = logPos[i]; t.localScale = logScale[i];
            LogPlaced?.Invoke();
        }

        private IEnumerator FadeInFlames()
        {
            litRoot.SetActive(true);
            if (coals != null) coals.sharedMaterial = coalsHot;
            flames.Intensity = 0f;
            for (float e = 0f; e < fadeInSeconds; e += Time.deltaTime)
            {
                flames.Intensity = e / fadeInSeconds;
                yield return null;
            }
            flames.Intensity = 1f;
            lighting = null;
        }

        private void ApplyVisuals(CampfireState s)
        {
            bool prepared = s != CampfireState.Unlit;
            for (int i = 0; i < logs.Length; i++)
            {
                logs[i].gameObject.SetActive(prepared);
                logs[i].localPosition = logPos[i]; logs[i].localRotation = logRot[i]; logs[i].localScale = logScale[i];
            }
            bool lit = s == CampfireState.Lit;
            litRoot.SetActive(lit);
            if (!lit) flames.Intensity = 0f;
            if (ignitionSparks != null && s != CampfireState.Igniting) ignitionSparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (coals != null) coals.sharedMaterial = lit ? coalsHot : coalsCold;
        }

        private void StopRoutines()
        {
            StopAllCoroutines(); // includes the per-log drop coroutines
            placing = lighting = null;
        }

        private void SetState(CampfireState s)
        {
            if (State == s) return;
            State = s;
            StateChanged?.Invoke(s);
        }
    }
}
