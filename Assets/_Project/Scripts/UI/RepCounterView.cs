using ExerciseGame.Core.Exercise;
using TMPro;
using UnityEngine;

namespace CampfireAdventure.UI
{
    /// <summary>
    /// The "Repetitions 04 / 10" card with a progress bar. It only reads <see cref="ExerciseManager"/>;
    /// the number pops briefly each time a validated rep is counted.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RepCounterView : MonoBehaviour
    {
        [SerializeField] private ExerciseManager exercise;
        [SerializeField] private TMP_Text countText;
        [SerializeField, Tooltip("Stretched from the left edge: its right anchor is the progress (0..1).")] private RectTransform barFill;

        [Header("Pop")]
        [SerializeField, Min(1f)] private float popScale = 1.25f;
        [SerializeField, Min(0.01f)] private float popSeconds = 0.25f;

        private int shownReps = -1;
        private int shownTarget = -1;
        private float popTimer;

        private void OnEnable()
        {
            if (exercise != null) exercise.RepCompleted += OnRep;
            Refresh(true);
        }

        private void OnDisable()
        {
            if (exercise != null) exercise.RepCompleted -= OnRep;
        }

        private void OnRep(int total) => popTimer = popSeconds;

        private void Update()
        {
            Refresh(false);

            if (popTimer > 0f)
            {
                popTimer = Mathf.Max(0f, popTimer - Time.unscaledDeltaTime);
                float k = popTimer / popSeconds;                       // 1 -> 0
                countText.rectTransform.localScale = Vector3.one * Mathf.Lerp(1f, popScale, Mathf.Sin(k * Mathf.PI));
            }
        }

        private void Refresh(bool force)
        {
            if (exercise == null) return;

            int target = Mathf.Max(1, exercise.TargetRepsPerSet * exercise.TargetSets);
            int reps = Mathf.Clamp(exercise.TotalReps, 0, target);
            if (!force && reps == shownReps && target == shownTarget) return;

            shownReps = reps; shownTarget = target;
            countText.text = $"{reps:00} <size=70%><color=#FFFFFFCC>/</color> {target}</size>";
            if (barFill != null) barFill.anchorMax = new Vector2((float)reps / target, 1f);
        }
    }
}
