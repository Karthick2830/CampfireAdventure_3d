using System;
using System.Collections;
using System.Text;
using CampfireAdventure.Core;
using CampfireAdventure.Objectives;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CampfireAdventure.UI
{
    /// <summary>
    /// The "Adventure Complete" panel. It appears only after the last objective (the lit campfire) is done,
    /// a moment later so the fire and celebration are seen first, with the results and two buttons.
    /// It is hidden again by a restart.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CompletionPanelView : MonoBehaviour
    {
        [SerializeField] private ObjectiveManager objectives;
        [SerializeField] private AdventureStats stats;
        [SerializeField] private AdventureSession session;
        [SerializeField, Tooltip("Optional: adds the validated rep count to the results.")] private ExerciseGame.Core.Exercise.ExerciseManager exercise;

        [Header("UI")]
        [SerializeField] private CanvasGroup group;
        [SerializeField] private TMP_Text resultsText;
        [SerializeField] private Button playAgainButton;
        [SerializeField, Tooltip("Hidden until a title screen exists (Phase 8).")] private Button returnToTitleButton;
        [SerializeField] private bool titleAvailable;

        [Header("Timing")]
        [SerializeField, Min(0f), Tooltip("Seconds after the fire is lit before the panel appears.")] private float showDelay = 3.2f;
        [SerializeField, Min(1f)] private float fadeSpeed = 3f;

        private readonly StringBuilder sb = new StringBuilder(160);
        private Coroutine routine;
        private float targetAlpha;

        public bool IsShowing => targetAlpha > 0f;

        /// <summary>Raised when "Return to Title" is pressed (wired to the title screen in Phase 8).</summary>
        public event Action ReturnToTitleRequested;

        private void Awake()
        {
            group.alpha = 0f; group.interactable = false; group.blocksRaycasts = false;
            playAgainButton.onClick.AddListener(OnPlayAgain);
            returnToTitleButton.onClick.AddListener(OnReturnToTitle);
            returnToTitleButton.gameObject.SetActive(titleAvailable);
        }

        private void OnEnable()
        {
            objectives.AllCompleted += OnAllCompleted;
            session.Restarted += Hide;
        }

        private void OnDisable()
        {
            objectives.AllCompleted -= OnAllCompleted;
            session.Restarted -= Hide;
        }

        private void Update()
        {
            group.alpha = Mathf.MoveTowards(group.alpha, targetAlpha, fadeSpeed * Time.unscaledDeltaTime);
            if (targetAlpha > 0f && group.interactable && EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
                EventSystem.current.SetSelectedGameObject(playAgainButton.gameObject);
        }

        private void OnAllCompleted()
        {
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(ShowAfterDelay());
        }

        private IEnumerator ShowAfterDelay()
        {
            yield return new WaitForSeconds(showDelay);
            sb.Clear()
              .Append("Time:  <b>").Append(stats.FormatElapsed()).Append("</b>\n")
              .Append("Wood gathered:  <b>").Append(stats.WoodCollected).Append("</b>\n")
              .Append("Stones gathered:  <b>").Append(stats.StonesCollected).Append("</b>\n")
              .Append("Ignition tries:  <b>").Append(stats.IgniteAttempts).Append("</b>");
            if (exercise != null)
            {
                int target = exercise.TargetRepsPerSet * exercise.TargetSets;
                sb.Append("\nTotal reps:  <b>").Append(exercise.TotalReps).Append(" / ").Append(target).Append("</b>");
            }
            resultsText.text = sb.ToString();

            targetAlpha = 1f;
            group.interactable = true; group.blocksRaycasts = true;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(playAgainButton.gameObject);
            routine = null;
        }

        private void Hide()
        {
            if (routine != null) StopCoroutine(routine);
            routine = null;
            targetAlpha = 0f;
            group.alpha = 0f; group.interactable = false; group.blocksRaycasts = false;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }

        private void OnPlayAgain() => session.RestartAdventure();
        private void OnReturnToTitle() => ReturnToTitleRequested?.Invoke();
    }
}
