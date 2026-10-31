using System.Text;
using CampfireAdventure.Core;
using CampfireAdventure.GameInput;
using CampfireAdventure.Interaction;
using TMPro;
using UnityEngine;

namespace CampfireAdventure.UI
{
    /// <summary>
    /// Bottom-centre contextual prompt ("E  Chop wood (1/2)"). Shown for the nearest interactable while the
    /// explorer is idle; greyed with the reason when the action is not available. Re-renders only on events.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InteractionPromptView : MonoBehaviour
    {
        [SerializeField] private PlayerInteractor interactor;
        [SerializeField] private GameActionController actions;
        [SerializeField] private KeyboardActionSource keys;
        [SerializeField] private TMP_Text text;
        [SerializeField] private CanvasGroup group;
        [SerializeField, Min(1f)] private float fadeSpeed = 8f;

        private readonly StringBuilder sb = new StringBuilder(128);
        private float targetAlpha;

        private void OnEnable()
        {
            interactor.CurrentChanged += OnCurrentChanged;
            interactor.CurrentUpdated += Refresh;
            actions.StateChanged += OnActionState;
            group.alpha = 0f;
            Refresh();
        }

        private void OnDisable()
        {
            interactor.CurrentChanged -= OnCurrentChanged;
            interactor.CurrentUpdated -= Refresh;
            actions.StateChanged -= OnActionState;
        }

        private void OnCurrentChanged(IInteractable _) => Refresh();
        private void OnActionState(ActionState _) => Refresh();

        private void Update()
        {
            group.alpha = Mathf.MoveTowards(group.alpha, targetAlpha, fadeSpeed * Time.unscaledDeltaTime);
        }

        private void Refresh()
        {
            var current = interactor.Current;
            if (current == null || actions.IsBusy)
            {
                targetAlpha = 0f;
                return;
            }

            targetAlpha = 1f;
            string key = keys != null ? keys.InteractLabel : "E";
            sb.Clear();
            if (current.CanInteract(out string reason))
            {
                sb.Append("<b><color=#FFD27A>").Append(key).Append("</color></b>   ").Append(current.Prompt);
            }
            else
            {
                sb.Append("<color=#9A9A9A><b>").Append(key).Append("</b>   ").Append(current.Prompt).Append("</color>\n")
                  .Append("<size=60%><color=#E0A060>").Append(reason).Append("</color></size>");
            }
            text.text = sb.ToString();
        }
    }
}
