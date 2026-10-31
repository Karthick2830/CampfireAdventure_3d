using System;
using CampfireAdventure.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CampfireAdventure.GameInput
{
    /// <summary>
    /// Keyboard source for the menu "back / pause" key. Kept separate from <see cref="KeyboardActionSource"/>
    /// because gameplay input is switched off while a menu is open, but this key must keep working.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MenuInputSource : MonoBehaviour, IMenuInputSource
    {
        [SerializeField, Tooltip("Pauses the game, or goes back one screen when a menu is open.")]
        private Key backKey = Key.Escape;

        public event Action MenuBackRequested;

        public string BackLabel => backKey == Key.Escape ? "Esc" : backKey.ToString();

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard[backKey].wasPressedThisFrame) MenuBackRequested?.Invoke();
        }
    }
}
