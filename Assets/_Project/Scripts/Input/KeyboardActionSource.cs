using System;
using System.Collections.Generic;
using CampfireAdventure.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CampfireAdventure.GameInput
{
    /// <summary>
    /// Prototype input source: one key press requests one named gameplay action.
    /// Uses the new Input System (the project has the old Input Manager disabled).
    /// Key bindings are configurable in the Inspector.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class KeyboardActionSource : MonoBehaviour, IGameActionSource, IInteractInputSource, ISessionInputSource
    {
        [Serializable]
        public struct KeyBinding
        {
            public Key key;
            public GameActionType action;
        }

        [SerializeField, Tooltip("Which key requests which action. Duplicate actions are allowed (e.g. add a numpad key).")]
        private List<KeyBinding> bindings = new List<KeyBinding>
        {
            new KeyBinding { key = Key.Digit1, action = GameActionType.GoToLumberyard },
            new KeyBinding { key = Key.Digit2, action = GameActionType.CollectWood },
            new KeyBinding { key = Key.Digit3, action = GameActionType.GoToQuarry },
            new KeyBinding { key = Key.Digit4, action = GameActionType.CollectStones },
            new KeyBinding { key = Key.Digit5, action = GameActionType.ReturnToCampsite },
            new KeyBinding { key = Key.Digit6, action = GameActionType.PrepareCampfire },
            new KeyBinding { key = Key.Digit7, action = GameActionType.IgniteCampfire },
        };

        [SerializeField, Tooltip("Contextual key: performs the action of the interactable the explorer is standing at.")]
        private Key interactKey = Key.E;

        [SerializeField, Tooltip("Development shortcut: restart the adventure.")]
        private Key restartKey = Key.F5;

        public event Action<GameActionType> ActionRequested;
        public event Action RestartRequested;
        public event Action InteractRequested;

        /// <summary>Short label of the interact key ("E").</summary>
        public string InteractLabel => interactKey.ToString();

        /// <summary>Short label of the first key bound to an action ("1".."7"), or "?" if unbound.</summary>
        public string GetKeyLabel(GameActionType action)
        {
            for (int i = 0; i < bindings.Count; i++)
            {
                if (bindings[i].action != action) continue;
                string label = bindings[i].key.ToString();
                if (label.StartsWith("Digit")) label = label.Substring(5);
                else if (label.StartsWith("Numpad")) label = "Num" + label.Substring(6);
                return label;
            }
            return "?";
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard[restartKey].wasPressedThisFrame)
            {
                RestartRequested?.Invoke();
                return;
            }

            if (keyboard[interactKey].wasPressedThisFrame)
            {
                InteractRequested?.Invoke();
                return;
            }

            for (int i = 0; i < bindings.Count; i++)
            {
                if (keyboard[bindings[i].key].wasPressedThisFrame)
                {
                    ActionRequested?.Invoke(bindings[i].action);
                    return; // at most one request per frame
                }
            }
        }
    }
}
