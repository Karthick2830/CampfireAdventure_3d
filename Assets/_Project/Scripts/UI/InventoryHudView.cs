using System.Collections;
using CampfireAdventure.Gathering;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CampfireAdventure.UI
{
    /// <summary>
    /// Compact top-right inventory: an icon and "have/needed" for wood and for stones. The count text
    /// punches briefly whenever it changes. Updates only when the inventory raises Changed.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InventoryHudView : MonoBehaviour
    {
        [SerializeField] private ResourceInventory inventory;

        [Header("Wood row")]
        [SerializeField] private Image woodIcon;
        [SerializeField] private TMP_Text woodCount;

        [Header("Stone row")]
        [SerializeField] private Image stoneIcon;
        [SerializeField] private TMP_Text stoneCount;

        [SerializeField] private Color completeColor = new Color(1f, 0.82f, 0.4f, 1f);

        private int lastWood = -1, lastStones = -1;

        private void OnEnable()
        {
            inventory.Changed += Refresh;
            woodIcon.sprite = inventory.Definition(ResourceType.Wood).Icon;
            stoneIcon.sprite = inventory.Definition(ResourceType.Stone).Icon;
            Refresh();
        }

        private void OnDisable() => inventory.Changed -= Refresh;

        private void Refresh()
        {
            if (inventory.Wood != lastWood) { Show(woodCount, inventory.Wood, inventory.RequiredWood, lastWood >= 0); lastWood = inventory.Wood; }
            if (inventory.Stones != lastStones) { Show(stoneCount, inventory.Stones, inventory.RequiredStones, lastStones >= 0); lastStones = inventory.Stones; }
        }

        private void Show(TMP_Text label, int have, int need, bool animate)
        {
            label.text = have + "/" + need;
            label.color = have >= need ? completeColor : Color.white;
            if (animate && isActiveAndEnabled) StartCoroutine(Punch(label.transform));
        }

        private static IEnumerator Punch(Transform t)
        {
            for (float e = 0f; e < 0.25f; e += Time.unscaledDeltaTime)
            {
                float k = Mathf.Sin(e / 0.25f * Mathf.PI);
                t.localScale = Vector3.one * (1f + 0.35f * k);
                yield return null;
            }
            t.localScale = Vector3.one;
        }
    }
}
