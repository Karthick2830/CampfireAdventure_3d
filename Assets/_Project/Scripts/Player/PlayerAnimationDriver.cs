using CampfireAdventure.Core;
using UnityEngine;

namespace CampfireAdventure.Player
{
    /// <summary>
    /// Feeds the Animator: planar speed for the idle/walk/run blend and triggers for one-shot actions.
    /// Does not decide when to play anything; <see cref="GameActionController"/> calls <see cref="Play"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerAnimationDriver : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int PickupHash = Animator.StringToHash("Pickup");
        private static readonly int VictoryHash = Animator.StringToHash("Victory");
        private static readonly int WaveHash = Animator.StringToHash("Wave");

        [SerializeField, Tooltip("Animator on the visual model (child of the player).")]
        private Animator animator;

        [SerializeField, Tooltip("Source of the current movement speed.")]
        private AutoNavigationController navigation;

        [SerializeField, Tooltip("Procedural axe swing used for the AxeChop animation (optional).")]
        private AxeSwingAnimator axeSwing;

        [SerializeField, Min(0f), Tooltip("Smoothing time for the speed parameter.")]
        private float speedDamping = 0.08f;

        private void Reset()
        {
            animator = GetComponentInChildren<Animator>();
            navigation = GetComponent<AutoNavigationController>();
            axeSwing = GetComponent<AxeSwingAnimator>();
        }

        private void Update()
        {
            if (animator == null || navigation == null) return;
            animator.SetFloat(SpeedHash, navigation.CurrentSpeed, speedDamping, Time.deltaTime);
        }

        /// <summary>Plays a one-shot action animation (None does nothing).</summary>
        public void Play(PlayerActionAnimation animation)
        {
            if (animator == null) return;
            switch (animation)
            {
                case PlayerActionAnimation.Pickup: animator.SetTrigger(PickupHash); break;
                case PlayerActionAnimation.Victory: animator.SetTrigger(VictoryHash); break;
                case PlayerActionAnimation.Wave: animator.SetTrigger(WaveHash); break;
                case PlayerActionAnimation.AxeChop: if (axeSwing != null) axeSwing.Begin(); break;
            }
        }

        /// <summary>Clears pending triggers and stops procedural poses (used when an action is cancelled).</summary>
        public void ResetTriggers()
        {
            if (animator == null) return;
            animator.ResetTrigger(PickupHash);
            animator.ResetTrigger(VictoryHash);
            animator.ResetTrigger(WaveHash);
            if (axeSwing != null) axeSwing.Stop();
        }
    }
}
