using System.Collections;
using UnityEngine;

namespace CampfireAdventure.Gathering
{
    /// <summary>
    /// Shows what the explorer is carrying on their back: one log bundle per collected wood and one
    /// stone per collected stone. Pieces pop in when collected and vanish when placed at the campfire.
    /// The pieces are ordinary child objects (set up in the scene), parented to a spine bone.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CarriedResourcesView : MonoBehaviour
    {
        [SerializeField] private ResourceInventory inventory;
        [SerializeField, Tooltip("Shown in order: the first N are visible when N wood is carried.")] private Transform[] woodPieces = new Transform[0];
        [SerializeField, Tooltip("Shown in order: the first N are visible when N stones are carried.")] private Transform[] stonePieces = new Transform[0];
        [SerializeField, Min(0.05f)] private float popSeconds = 0.3f;

        private Vector3[] woodScale, stoneScale;
        private int shownWood, shownStones;

        private void Awake()
        {
            woodScale = Capture(woodPieces);
            stoneScale = Capture(stonePieces);
            Apply(woodPieces, woodScale, 0, 0, false);
            Apply(stonePieces, stoneScale, 0, 0, false);
        }

        private void OnEnable()
        {
            inventory.Changed += Refresh;
            Refresh();
        }

        private void OnDisable() => inventory.Changed -= Refresh;

        private void Refresh()
        {
            Apply(woodPieces, woodScale, shownWood, inventory.Wood, true);
            Apply(stonePieces, stoneScale, shownStones, inventory.Stones, true);
            shownWood = inventory.Wood;
            shownStones = inventory.Stones;
        }

        private void Apply(Transform[] pieces, Vector3[] scales, int from, int to, bool animate)
        {
            for (int i = 0; i < pieces.Length; i++)
            {
                bool shouldShow = i < to;
                bool wasShown = i < from;
                if (pieces[i] == null || shouldShow == wasShown && pieces[i].gameObject.activeSelf == shouldShow) continue;
                if (shouldShow && animate && isActiveAndEnabled) { pieces[i].gameObject.SetActive(true); StartCoroutine(Pop(pieces[i], scales[i])); }
                else { pieces[i].gameObject.SetActive(shouldShow); pieces[i].localScale = scales[i]; }
            }
        }

        private IEnumerator Pop(Transform t, Vector3 target)
        {
            for (float e = 0f; e < popSeconds; e += Time.deltaTime)
            {
                float k = e / popSeconds;
                float overshoot = 1f + 0.3f * Mathf.Sin(k * Mathf.PI);
                t.localScale = target * Mathf.Clamp01(k * 1.4f) * overshoot;
                yield return null;
            }
            t.localScale = target;
        }

        private static Vector3[] Capture(Transform[] ts)
        {
            var s = new Vector3[ts.Length];
            for (int i = 0; i < ts.Length; i++) s[i] = ts[i] != null ? ts[i].localScale : Vector3.one;
            return s;
        }
    }
}
