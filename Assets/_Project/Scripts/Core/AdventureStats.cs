using CampfireAdventure.Campfire;
using CampfireAdventure.Gathering;
using CampfireAdventure.Objectives;
using UnityEngine;

namespace CampfireAdventure.Core
{
    /// <summary>
    /// Numbers for the completion screen: how long the adventure took, what was gathered, how many
    /// ignition attempts it needed. The clock runs from the start (or restart) until the last objective.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AdventureStats : MonoBehaviour
    {
        [SerializeField] private ObjectiveManager objectives;
        [SerializeField] private ResourceInventory inventory;
        [SerializeField] private CampfireController campfire;
        [SerializeField] private AdventureSession session;

        private float startTime;
        private float endTime = -1f;

        public bool IsFinished => endTime >= 0f;
        public float ElapsedSeconds => (IsFinished ? endTime : Time.time) - startTime;
        public int WoodCollected => inventory.TotalWoodCollected;
        public int StonesCollected => inventory.TotalStonesCollected;
        public int IgniteAttempts => campfire.IgniteAttempts;

        private void OnEnable()
        {
            objectives.AllCompleted += OnAllCompleted;
            session.Restarted += OnRestarted;
            startTime = Time.time;
        }

        private void OnDisable()
        {
            objectives.AllCompleted -= OnAllCompleted;
            session.Restarted -= OnRestarted;
        }

        private void OnAllCompleted() => endTime = Time.time;

        /// <summary>Restarts the clock (used when the player presses Play on the title screen).</summary>
        public void ResetClock()
        {
            startTime = Time.time;
            endTime = -1f;
        }

        private void OnRestarted()
        {
            startTime = Time.time;
            endTime = -1f;
        }

        /// <summary>Elapsed time as "m:ss".</summary>
        public string FormatElapsed()
        {
            int total = Mathf.FloorToInt(ElapsedSeconds);
            return (total / 60) + ":" + (total % 60).ToString("00");
        }
    }
}
