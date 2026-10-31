using System;
using UnityEngine;
using UnityEngine.UI;

namespace CampfireAdventure.UI
{
    /// <summary>Title screen: Play, Settings, Quit. It only raises events; the game flow decides what they do.</summary>
    public sealed class TitleScreenView : UiScreen
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        public event Action PlayRequested;
        public event Action SettingsRequested;
        public event Action QuitRequested;

        protected override void Awake()
        {
            base.Awake();
            playButton.onClick.AddListener(() => PlayRequested?.Invoke());
            settingsButton.onClick.AddListener(() => SettingsRequested?.Invoke());
            quitButton.onClick.AddListener(() => QuitRequested?.Invoke());
        }
    }
}
