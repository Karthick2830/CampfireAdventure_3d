using System;
using UnityEngine;

namespace CampfireAdventure.Player
{
    /// <summary>
    /// Procedural overhead axe swings on the humanoid rig (the packs have no chopping clip).
    /// The axe is lifted overhead first, then each swing strikes DOWN and the axe is raised again for the
    /// next one. Runs in LateUpdate, after the Animator, rotating the spine and both arms about the
    /// character's right axis (negative angle = arm forward/up). Shows the axe prop while swinging and
    /// raises <see cref="Impact"/> on every hit.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AxeSwingAnimator : MonoBehaviour
    {
        [SerializeField, Tooltip("Humanoid Animator whose bones are posed.")]
        private Animator animator;

        [SerializeField, Tooltip("Axe prop parented to the right hand. Shown only while swinging.")]
        private GameObject axeProp;

        [Header("Timing (seconds)")]
        [SerializeField, Min(1)] private int swings = 3;
        [SerializeField, Min(0.05f), Tooltip("Initial lift from the resting arms to the axe held overhead.")]
        private float liftSeconds = 0.4f;
        [SerializeField, Min(0.05f), Tooltip("The downward strike.")]
        private float strikeSeconds = 0.12f;
        [SerializeField, Min(0.1f), Tooltip("Raising the axe back overhead after a hit (the last swing relaxes to rest instead).")]
        private float recoverSeconds = 0.55f;
        [SerializeField, Min(0.01f), Tooltip("Fade-out at the very end.")]
        private float blendSeconds = 0.18f;

        [Header("Pose (degrees about the character's right axis)")]
        [SerializeField, Tooltip("Arm angle with the axe overhead. Negative = forward/up.")]
        private float raisedArm = -150f;
        [SerializeField, Tooltip("Arm angle at the moment of impact. Negative = forward/down.")]
        private float strikeArm = -30f;
        [SerializeField] private float raisedElbow = 70f;
        [SerializeField] private float strikeElbow = 15f;
        [SerializeField, Tooltip("Spine lean when raised (negative = back).")] private float raisedLean = -8f;
        [SerializeField, Tooltip("Spine lean on impact (positive = forward).")] private float strikeLean = 14f;
        [SerializeField, Range(0f, 1f), Tooltip("How much the left arm follows the right (two-handed grip).")]
        private float leftArmFollow = 0.9f;

        private bool playing;
        private float startTime;
        private int impactsFired;
        private Transform spine, rUpper, rLower, lUpper, lLower;

        /// <summary>Raised on each hit with (hitIndex, totalHits).</summary>
        public event Action<int, int> Impact;

        public bool IsPlaying => playing;
        public int Swings => swings;
        private float Period => strikeSeconds + recoverSeconds;
        public float TotalSeconds => liftSeconds + swings * Period;
        /// <summary>Seconds after Begin() at which the last hit lands.</summary>
        public float LastImpactSeconds => liftSeconds + (swings - 1) * Period + strikeSeconds;

        private void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            spine = animator.GetBoneTransform(HumanBodyBones.Spine);
            rUpper = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            rLower = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            lUpper = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            lLower = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            if (axeProp != null) axeProp.SetActive(false);
        }

        public void Begin()
        {
            playing = true;
            startTime = Time.time;
            impactsFired = 0;
            if (axeProp != null) axeProp.SetActive(true);
        }

        public void Stop()
        {
            playing = false;
            if (axeProp != null) axeProp.SetActive(false);
        }

        private void LateUpdate()
        {
            if (!playing) return;

            float t = Time.time - startTime;
            float total = TotalSeconds;
            if (t >= total + blendSeconds) { Stop(); return; }

            EvaluatePose(Mathf.Min(t, total), out float arm, out float elbow, out float lean);
            float weight = Mathf.Clamp01((total + blendSeconds - t) / blendSeconds);
            Apply(arm, elbow, lean, weight);

            while (impactsFired < swings && t >= liftSeconds + impactsFired * Period + strikeSeconds)
            {
                Impact?.Invoke(impactsFired, swings);
                impactsFired++;
            }
        }

        private void EvaluatePose(float t, out float arm, out float elbow, out float lean)
        {
            if (t < liftSeconds)
            {
                // arms hanging -> axe overhead
                float s = Mathf.SmoothStep(0f, 1f, t / liftSeconds);
                arm = Mathf.Lerp(0f, raisedArm, s); elbow = Mathf.Lerp(0f, raisedElbow, s); lean = Mathf.Lerp(0f, raisedLean, s);
                return;
            }

            float u = t - liftSeconds;
            int k = Mathf.Clamp((int)(u / Period), 0, swings - 1);
            float v = u - k * Period;
            bool last = k == swings - 1;

            if (v < strikeSeconds)
            {
                // overhead -> down onto the target, accelerating into the hit
                float s = v / strikeSeconds; s *= s;
                arm = Mathf.Lerp(raisedArm, strikeArm, s); elbow = Mathf.Lerp(raisedElbow, strikeElbow, s); lean = Mathf.Lerp(raisedLean, strikeLean, s);
            }
            else
            {
                // back overhead for the next swing (or relax to rest after the last one)
                float s = Mathf.SmoothStep(0f, 1f, (v - strikeSeconds) / recoverSeconds);
                arm = Mathf.Lerp(strikeArm, last ? 0f : raisedArm, s);
                elbow = Mathf.Lerp(strikeElbow, last ? 0f : raisedElbow, s);
                lean = Mathf.Lerp(strikeLean, last ? 0f : raisedLean, s);
            }
        }

        private void Apply(float arm, float elbow, float lean, float weight)
        {
            Vector3 axis = animator.transform.right;
            Rotate(spine, lean * weight, axis);
            Rotate(rUpper, arm * weight, axis);
            Rotate(rLower, -elbow * weight, axis);
            Rotate(lUpper, arm * leftArmFollow * weight, axis);
            Rotate(lLower, -elbow * leftArmFollow * weight, axis);
        }

        private static void Rotate(Transform bone, float angle, Vector3 axis)
        {
            if (bone != null) bone.rotation = Quaternion.AngleAxis(angle, axis) * bone.rotation;
        }
    }
}
