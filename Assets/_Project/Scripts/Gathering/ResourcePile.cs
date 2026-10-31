using System.Collections;
using UnityEngine;

namespace CampfireAdventure.Gathering
{
    /// <summary>
    /// Visual depletion of one collection point. Each collection removes one "stage" of the pile
    /// (shrinking the pieces away with a burst of debris); axe impacts add chips, dust and sparks.
    /// Holds no gameplay state beyond how many stages were taken.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ResourcePile : MonoBehaviour
    {
        [SerializeField] private ResourceType type;

        [Header("Stages (removed in order)")]
        [SerializeField] private Transform[] stage1 = new Transform[0];
        [SerializeField] private Transform[] stage2 = new Transform[0];

        [Header("Effects (all optional)")]
        [SerializeField, Tooltip("Mesh-chip burst (rock chips or wood splinters).")]
        private ParticleSystem chips;
        [SerializeField] private ParticleSystem dust;
        [SerializeField, Tooltip("Bright flint sparks on axe hits.")]
        private ParticleSystem sparks;

        [SerializeField, Min(0.05f)] private float shrinkSeconds = 0.3f;

        private Vector3[] stage1Scale, stage2Scale;
        private int taken;

        public ResourceType Type => type;
        public int StagesTaken => taken;

        private void Awake()
        {
            stage1Scale = Capture(stage1);
            stage2Scale = Capture(stage2);
        }

        /// <summary>Small hit effects while the axe is swinging (index 0..total-1).</summary>
        public void PlayImpact(int index, int total)
        {
            float t = total <= 1 ? 1f : (float)index / (total - 1);
            if (chips != null) chips.Emit(Mathf.RoundToInt(Mathf.Lerp(5, 10, t)));
            if (dust != null) dust.Emit(Mathf.RoundToInt(Mathf.Lerp(2, 4, t)));
            if (sparks != null) sparks.Emit(Mathf.RoundToInt(Mathf.Lerp(6, 12, t)));
            StartCoroutine(Punch());
        }

        /// <summary>Removes the next stage of the pile with a larger burst of debris.</summary>
        public void TakeStage()
        {
            Transform[] stage = taken == 0 ? stage1 : taken == 1 ? stage2 : null;
            Vector3[] scales = taken == 0 ? stage1Scale : stage2Scale;
            taken++;
            if (stage == null) return;

            if (chips != null) chips.Emit(type == ResourceType.Stone ? 34 : 14);
            if (dust != null) dust.Emit(type == ResourceType.Stone ? 14 : 6);
            if (sparks != null && type == ResourceType.Stone) sparks.Emit(10);
            StartCoroutine(ShrinkAway(stage));
        }

        /// <summary>True once every stage has been taken.</summary>
        public bool Depleted => taken >= 2;

        /// <summary>Removes the whole pile in one big burst of debris (a point with a single unit).</summary>
        public void TakeAll()
        {
            if (Depleted) return;
            if (chips != null) chips.Emit(type == ResourceType.Stone ? 46 : 22);
            if (dust != null) dust.Emit(type == ResourceType.Stone ? 18 : 8);
            if (sparks != null && type == ResourceType.Stone) sparks.Emit(12);
            if (taken == 0) StartCoroutine(ShrinkAway(stage1));
            StartCoroutine(ShrinkAway(stage2));
            taken = 2;
        }

        /// <summary>Restores every piece (used when the adventure restarts).</summary>
        public void ResetPile()
        {
            StopAllCoroutines();
            taken = 0;
            Restore(stage1, stage1Scale);
            Restore(stage2, stage2Scale);
        }

        private IEnumerator ShrinkAway(Transform[] pieces)
        {
            var from = Capture(pieces);
            for (float t = 0f; t < shrinkSeconds; t += Time.deltaTime)
            {
                float k = 1f - Mathf.Pow(1f - t / shrinkSeconds, 2f);
                for (int i = 0; i < pieces.Length; i++)
                    if (pieces[i] != null) pieces[i].localScale = from[i] * (1f - k);
                yield return null;
            }
            foreach (var p in pieces) if (p != null) p.gameObject.SetActive(false);
        }

        private IEnumerator Punch()
        {
            Vector3 baseScale = transform.localScale;
            for (float t = 0f; t < 0.14f; t += Time.deltaTime)
            {
                float k = Mathf.Sin(t / 0.14f * Mathf.PI);
                transform.localScale = baseScale * (1f + 0.06f * k);
                yield return null;
            }
            transform.localScale = baseScale;
        }

        private static Vector3[] Capture(Transform[] ts)
        {
            var s = new Vector3[ts.Length];
            for (int i = 0; i < ts.Length; i++) s[i] = ts[i] != null ? ts[i].localScale : Vector3.one;
            return s;
        }

        private static void Restore(Transform[] ts, Vector3[] scales)
        {
            for (int i = 0; i < ts.Length; i++)
            {
                if (ts[i] == null) continue;
                ts[i].gameObject.SetActive(true);
                ts[i].localScale = scales[i];
            }
        }
    }
}
