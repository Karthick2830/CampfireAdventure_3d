using System;
using UnityEngine;
using UnityEngine.UI;

namespace CampfireAdventure.UI
{
    /// <summary>Pause menu: Resume, Restart adventure, Settings, Return to title.</summary>
    public sealed class PauseMenuView : UiScreen
    {
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button titleButton;

        public event Action ResumeRequested;
        public event Action RestartRequested;
        public event Action SettingsRequested;
        public event Action TitleRequested;

        protected override void Awake()
        {
            base.Awake();
            resumeButton.onClick.AddListener(() => ResumeRequested?.Invoke());
            restartButton.onClick.AddListener(() => RestartRequested?.Invoke());
            settingsButton.onClick.AddListener(() => SettingsRequested?.Invoke());
            titleButton.onClick.AddListener(() => TitleRequested?.Invoke());
        }
    }
}
