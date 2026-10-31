using CampfireAdventure.Audio;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CampfireAdventure.UI
{
    /// <summary>Add to any button, toggle or slider to give it the standard menu sounds.</summary>
    public sealed class UiSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, ISubmitHandler
    {
        [SerializeField, Tooltip("Play the softer 'back' sound instead of the normal click.")] private bool isBack;
        [SerializeField, Tooltip("Hover sound only (sliders play no click).")] private bool hoverOnly;

        public void OnPointerEnter(PointerEventData eventData) => UiAudioPlayer.Instance?.PlayHover();

        public void OnPointerClick(PointerEventData eventData) => Click();

        public void OnSubmit(BaseEventData eventData) => Click();

        private void Click()
        {
            if (hoverOnly) return;
            var player = UiAudioPlayer.Instance;
            if (player == null) return;
            if (isBack) player.PlayBack(); else player.PlayClick();
        }
    }
}
