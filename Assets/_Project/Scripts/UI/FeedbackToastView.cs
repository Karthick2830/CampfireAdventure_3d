using CampfireAdventure.Core;
using CampfireAdventure.Campfire;
using CampfireAdventure.Gathering;
using CampfireAdventure.Interaction;
using CampfireAdventure.Objectives;
using TMPro;
using UnityEngine;

namespace CampfireAdventure.UI
{
    /// <summary>
    /// A single short message near the top of the screen: pickups, refusals and their reasons, ignition
    /// hints and "objective complete". A new message replaces the old one; it fades after a few seconds.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FeedbackToastView : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private CanvasGroup group;
        [SerializeField, Min(0.5f)] private float seconds = 3.2f;
        [SerializeField, Min(1f)] private float fadeSpeed = 6f;

        [Header("Sources (all optional)")]
        [SerializeField] private GameActionController actions;
        [SerializeField] private PlayerInteractor interactor;
        [SerializeField] private ResourceCollector collector;
        [SerializeField] private CampfireController campfire;
        [SerializeField] private ObjectiveManager objectives;
        [SerializeField] private AdventureSession session;

        private float hideAt;
        private float targetAlpha;

        private void OnEnable()
        {
            if (actions != null) actions.ActionRejected += OnRejected;
            if (interactor != null) interactor.Notice += ShowInfo;
            if (collector != null) collector.Notice += ShowInfo;
            if (campfire != null) campfire.Notice += ShowWarning;
            if (objectives != null) { objectives.ObjectiveCompleted += OnObjectiveCompleted; objectives.AllCompleted += OnAllCompleted; }
            if (session != null) session.Restarted += OnRestarted;
            group.alpha = 0f;
        }

        private void OnDisable()
        {
            if (actions != null) actions.ActionRejected -= OnRejected;
            if (interactor != null) interactor.Notice -= ShowInfo;
            if (collector != null) collector.Notice -= ShowInfo;
            if (campfire != null) campfire.Notice -= ShowWarning;
            if (objectives != null) { objectives.ObjectiveCompleted -= OnObjectiveCompleted; objectives.AllCompleted -= OnAllCompleted; }
            if (session != null) session.Restarted -= OnRestarted;
        }

        private void Update()
        {
            if (targetAlpha > 0f && Time.unscaledTime > hideAt) targetAlpha = 0f;
            group.alpha = Mathf.MoveTowards(group.alpha, targetAlpha, fadeSpeed * Time.unscaledDeltaTime);
        }

        private void OnRejected(GameActionType action, string reason) => ShowWarning(reason);
        private void OnObjectiveCompleted(ObjectiveDefinition def, int index) => Show(def.CompletionMessage, "#9BE08A");
        private void OnAllCompleted() => Show("The campfire is burning. Adventure complete!", "#FFD27A");
        private void OnRestarted() => Show("A new adventure begins.", "#FFFFFF");

        private void ShowInfo(string message) => Show(message, "#FFFFFF");
        private void ShowWarning(string message) => Show(message, "#FFB070");

        private void Show(string message, string colorHex)
        {
            text.text = "<color=" + colorHex + ">" + message + "</color>";
            targetAlpha = 1f;
            hideAt = Time.unscaledTime + seconds;
        }
    }
}
