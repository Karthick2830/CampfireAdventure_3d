using CampfireAdventure.Player;
using UnityEngine;

namespace CampfireAdventure.Core
{
    /// <summary>
    /// Drives the evening-to-night change: as the explorer heads back to the campfire (return, prepare
    /// or ignite), the light fades toward night in proportion to how close they have got, and is fully
    /// night on arrival. Starting at the campsite fades over a fixed time instead.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TimeOfDayDirector : MonoBehaviour
    {
        [SerializeField] private GameActionController actions;
        [SerializeField] private LightingMoodController mood;
        [SerializeField] private AutoNavigationController player;
        [SerializeField, Tooltip("Where 'home' is; the fade is measured against the distance to this point.")]
        private Transform campfireAnchor;

        [SerializeField, Min(1f), Tooltip("If the trip starts closer than this, fade over a fixed time instead of by distance.")]
        private float minTripForDistanceFade = 6f;
        [SerializeField, Min(0.1f), Tooltip("Seconds for the fixed-time fade and for the final settle on arrival.")]
        private float settleSeconds = 1.5f;

        private bool tracking;
        private float startDistance;

        private void OnEnable()
        {
            actions.ActionStarted += OnStarted;
            actions.ActionArrived += OnArrived;
        }

        private void OnDisable()
        {
            actions.ActionStarted -= OnStarted;
            actions.ActionArrived -= OnArrived;
        }

        /// <summary>Back to the evening look (used by restart).</summary>
        public void ResetTime()
        {
            tracking = false;
            mood.SetBlend(0f);
        }

        private static bool GoesHome(GameActionType a) =>
            a == GameActionType.ReturnToCampsite || a == GameActionType.PrepareCampfire || a == GameActionType.IgniteCampfire;

        private void OnStarted(GameActionType action)
        {
            if (!GoesHome(action) || mood.Blend >= 0.999f) return;
            startDistance = player.DistanceTo(campfireAnchor.position);
            if (startDistance < minTripForDistanceFade)
            {
                tracking = false;
                mood.FadeTo(1f, settleSeconds + 1f);
            }
            else tracking = true;
        }

        private void OnArrived(GameActionType action)
        {
            if (!GoesHome(action)) return;
            tracking = false;
            mood.FadeTo(1f, settleSeconds);
        }

        private void Update()
        {
            if (!tracking) return;
            if (actions.State != ActionState.Navigating || !actions.CurrentAction.HasValue || !GoesHome(actions.CurrentAction.Value))
            {
                tracking = false; // trip was cancelled or failed: keep whatever light level we reached
                return;
            }
            float t = 1f - Mathf.Clamp01(player.DistanceTo(campfireAnchor.position) / startDistance);
            float target = Mathf.SmoothStep(0f, 1f, t);
            if (target > mood.Blend) mood.SetBlend(target); // never gets brighter while walking home
        }
    }
}
