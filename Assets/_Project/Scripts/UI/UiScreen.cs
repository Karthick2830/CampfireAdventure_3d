using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CampfireAdventure.UI
{
    /// <summary>
    /// Base for a full menu screen (title, pause, settings): a CanvasGroup that fades in and out using
    /// unscaled time (so it works while the game is paused), blocks clicks only while open, and puts keyboard
    /// focus on its first button when it opens.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UiScreen : MonoBehaviour
    {
        [SerializeField] private Selectable firstSelected;
        [SerializeField, Min(1f)] private float fadeSpeed = 6f;

        private CanvasGroup group;
        private float target;

        public bool IsOpen { get; private set; }

        protected virtual void Awake()
        {
            group = GetComponent<CanvasGroup>();
            group.alpha = 0f; group.interactable = false; group.blocksRaycasts = false;
        }

        public virtual void Show()
        {
            IsOpen = true; target = 1f;
            group.interactable = true; group.blocksRaycasts = true;
            Focus();
        }

        public virtual void Hide()
        {
            IsOpen = false; target = 0f;
            group.interactable = false; group.blocksRaycasts = false;
        }

        /// <summary>
        /// Used while another screen sits on top: false stops clicks and keyboard navigation and fades this
        /// screen out so it does not show through; true brings it back.
        /// </summary>
        public void SetInteractable(bool value)
        {
            group.interactable = value && IsOpen;
            group.blocksRaycasts = value && IsOpen;
            target = value && IsOpen ? 1f : 0f;
        }

        public void Focus()
        {
            if (firstSelected != null && EventSystem.current != null) EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);
        }

        protected virtual void Update()
        {
            group.alpha = Mathf.MoveTowards(group.alpha, target, fadeSpeed * Time.unscaledDeltaTime);
            // keep keyboard navigation alive: if focus was lost (e.g. a click on empty space), put it back
            if (IsOpen && group.interactable && EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null) Focus();
        }
    }
}
