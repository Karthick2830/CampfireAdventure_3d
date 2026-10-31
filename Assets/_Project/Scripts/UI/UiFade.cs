using UnityEngine;

namespace CampfireAdventure.UI
{
    /// <summary>Fades a group of HUD elements in or out (unscaled time) and disables clicks while hidden.</summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class UiFade : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float fadeSpeed = 5f;

        private CanvasGroup group;
        private float target;

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
        }

        public void SetVisible(bool visible, bool instant = false)
        {
            if (group == null) group = GetComponent<CanvasGroup>();
            target = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
            if (instant) group.alpha = target;
        }

        private void Update()
        {
            group.alpha = Mathf.MoveTowards(group.alpha, target, fadeSpeed * Time.unscaledDeltaTime);
        }
    }
}
