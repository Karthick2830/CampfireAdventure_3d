using System.Collections;
using CampfireAdventure.Core;
using CampfireAdventure.Objectives;
using CampfireAdventure.Player;
using UnityEngine;

namespace CampfireAdventure.Campfire
{
    /// <summary>
    /// The reward moment when the last objective is done: a burst of golden sparks above the fire and the
    /// explorer cheering. Purely visual; the completion panel and sound are handled elsewhere.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CompletionFlourish : MonoBehaviour
    {
        [SerializeField] private ObjectiveManager objectives;
        [SerializeField] private PlayerAnimationDriver explorer;
        [SerializeField, Tooltip("Particle system used only through Emit().")] private ParticleSystem sparkBurst;
        [SerializeField, Min(0f), Tooltip("Wait for the flames to take hold first.")] private float delaySeconds = 0.8f;
        [SerializeField, Min(1)] private int burstCount = 70;

        private Coroutine routine;

        private void OnEnable() => objectives.AllCompleted += OnAllCompleted;

        private void OnDisable()
        {
            objectives.AllCompleted -= OnAllCompleted;
            if (routine != null) StopCoroutine(routine);
            routine = null;
        }

        private void OnAllCompleted()
        {
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(Flourish());
        }

        private IEnumerator Flourish()
        {
            yield return new WaitForSeconds(delaySeconds);
            explorer.Play(PlayerActionAnimation.Victory);
            if (sparkBurst != null)
            {
                sparkBurst.Emit(burstCount);
                yield return new WaitForSeconds(0.5f);
                sparkBurst.Emit(burstCount / 2);
            }
            routine = null;
        }
    }
}
