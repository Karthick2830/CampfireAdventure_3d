using System.Text;
using CampfireAdventure.Core;
using CampfireAdventure.GameInput;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CampfireAdventure.UI
{
    /// <summary>
    /// The small "controls" hint in the bottom-left corner: the key for each action, plus E, F5 and Esc.
    /// It can be switched off in Settings. Text is rebuilt only when something it shows changes.
    /// </summary>
    public sealed class ActionStatusLabel : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private Graphic background;
        [SerializeField] private GameActionController controller;
        [SerializeField] private KeyboardActionSource keys;
        [SerializeField] private MenuInputSource menuKeys;
        [SerializeField, Tooltip("Optional: lets the player hide this hint in Settings.")] private SettingsController settings;

        private static readonly GameActionType[] Order =
        {
            GameActionType.GoToLumberyard, GameActionType.CollectWood, GameActionType.GoToQuarry, GameActionType.CollectStones,
            GameActionType.ReturnToCampsite, GameActionType.PrepareCampfire, GameActionType.IgniteCampfire,
        };

        private readonly StringBuilder sb = new StringBuilder(256);

        private void OnEnable()
        {
            if (settings != null) settings.Changed += ApplyVisibility;
            ApplyVisibility();
        }

        private void OnDisable()
        {
            if (settings != null) settings.Changed -= ApplyVisibility;
        }

        // The controller builds its display-name table in Awake; build the text once everything has woken up.
        private void Start()
        {
            sb.Clear();
            for (int i = 0; i < Order.Length; i++)
            {
                sb.Append("<b><color=#FFD27A>").Append(keys.GetKeyLabel(Order[i])).Append("</color></b> ").Append(controller.GetDisplayName(Order[i]));
                sb.Append(i % 2 == 1 || i == Order.Length - 1 ? '\n' : "     ");
            }
            sb.Append("<b><color=#FFD27A>").Append(keys.InteractLabel).Append("</color></b> Interact     ")
              .Append("<b><color=#FFD27A>F5</color></b> Restart     ")
              .Append("<b><color=#FFD27A>").Append(menuKeys != null ? menuKeys.BackLabel : "Esc").Append("</color></b> Pause");
            label.text = sb.ToString();
        }

        private void ApplyVisibility()
        {
            bool show = settings == null || settings.ShowControlsHint;
            label.enabled = show;
            if (background != null) background.enabled = show;
        }
    }
}
