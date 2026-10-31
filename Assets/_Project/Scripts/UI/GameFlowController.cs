using System;
using System.Collections.Generic;
using CampfireAdventure.Core;
using UnityEngine;

namespace CampfireAdventure.UI
{
    public enum GameFlowState
    {
        Title,
        Playing,
        Paused,
    }

    /// <summary>
    /// The high-level state of the game: Title -> Playing <-> Paused. It decides which screen is visible,
    /// switches gameplay input on and off, and freezes time while paused. It holds no gameplay rules;
    /// a restart is delegated to <see cref="AdventureSession"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameFlowController : MonoBehaviour
    {
        [Header("Game systems")]
        [SerializeField] private AdventureSession session;
        [SerializeField] private AdventureStats stats;

        [Header("Input")]
        [SerializeField, Tooltip("Gameplay input sources. Enabled only while playing.")]
        private List<Behaviour> gameplayInputs = new List<Behaviour>();
        [SerializeField, Tooltip("Components implementing IMenuInputSource (Esc).")]
        private List<MonoBehaviour> menuInputs = new List<MonoBehaviour>();

        [Header("Screens")]
        [SerializeField] private TitleScreenView title;
        [SerializeField] private PauseMenuView pause;
        [SerializeField] private SettingsPanelView settings;
        [SerializeField] private CompletionPanelView completion;
        [SerializeField, Tooltip("All in-game HUD elements (hidden on the title screen).")] private UiFade gameplayHud;
        [SerializeField, Tooltip("The on-screen pause button in the HUD.")] private UnityEngine.UI.Button pauseButton;

        private UiScreen settingsReturnTo;

        public GameFlowState State { get; private set; } = GameFlowState.Title;
        public event Action<GameFlowState> StateChanged;

        private void OnEnable()
        {
            foreach (var mb in menuInputs)
            {
                if (mb is IMenuInputSource s) s.MenuBackRequested += OnMenuBack;
                else if (mb != null) Debug.LogError($"{mb.name} does not implement IMenuInputSource.", this);
            }
            title.PlayRequested += StartAdventure;
            title.SettingsRequested += OpenSettingsFromTitle;
            title.QuitRequested += Quit;
            pause.ResumeRequested += Resume;
            pause.RestartRequested += RestartFromPause;
            pause.SettingsRequested += OpenSettingsFromPause;
            pause.TitleRequested += ReturnToTitle;
            settings.BackRequested += CloseSettings;
            completion.ReturnToTitleRequested += ReturnToTitle;
            if (pauseButton != null) pauseButton.onClick.AddListener(PauseGame);
        }

        private void OnDisable()
        {
            foreach (var mb in menuInputs)
                if (mb is IMenuInputSource s) s.MenuBackRequested -= OnMenuBack;
            title.PlayRequested -= StartAdventure;
            title.SettingsRequested -= OpenSettingsFromTitle;
            title.QuitRequested -= Quit;
            pause.ResumeRequested -= Resume;
            pause.RestartRequested -= RestartFromPause;
            pause.SettingsRequested -= OpenSettingsFromPause;
            pause.TitleRequested -= ReturnToTitle;
            settings.BackRequested -= CloseSettings;
            completion.ReturnToTitleRequested -= ReturnToTitle;
            if (pauseButton != null) pauseButton.onClick.RemoveListener(PauseGame);
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }

        private void Start() => EnterTitle(instant: true);

        // ------------------------------------------------------------ transitions

        public void StartAdventure()
        {
            if (State != GameFlowState.Title) return;
            title.Hide();
            SetState(GameFlowState.Playing);
            stats.ResetClock();
        }

        public void PauseGame()
        {
            if (State != GameFlowState.Playing) return;
            SetState(GameFlowState.Paused);
            pause.Show();
        }

        public void Resume()
        {
            if (State != GameFlowState.Paused) return;
            settings.Hide();
            pause.Hide();
            SetState(GameFlowState.Playing);
        }

        public void ReturnToTitle()
        {
            pause.Hide(); settings.Hide();
            Time.timeScale = 1f; AudioListener.pause = false;
            session.RestartAdventure();
            EnterTitle(instant: false);
        }

        private void RestartFromPause()
        {
            Resume();
            session.RestartAdventure();
        }

        private void EnterTitle(bool instant)
        {
            SetState(GameFlowState.Title, instant);
            title.Show();
        }

        private void OpenSettingsFromTitle() => OpenSettings(title);
        private void OpenSettingsFromPause() => OpenSettings(pause);

        private void OpenSettings(UiScreen from)
        {
            settingsReturnTo = from;
            from.SetInteractable(false);
            settings.Show();
        }

        private void CloseSettings()
        {
            settings.Hide();
            if (settingsReturnTo != null)
            {
                settingsReturnTo.SetInteractable(true);
                settingsReturnTo.Focus();
                settingsReturnTo = null;
            }
        }

        private void OnMenuBack()
        {
            if (settings.IsOpen) CloseSettings();
            else if (State == GameFlowState.Paused) Resume();
            else if (State == GameFlowState.Playing) PauseGame();
        }

        private void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ------------------------------------------------------------ state

        private void SetState(GameFlowState state, bool instant = false)
        {
            State = state;
            bool playing = state == GameFlowState.Playing;
            foreach (var input in gameplayInputs) if (input != null) input.enabled = playing;
            gameplayHud.SetVisible(state != GameFlowState.Title, instant);
            Time.timeScale = state == GameFlowState.Paused ? 0f : 1f;
            AudioListener.pause = state == GameFlowState.Paused;
            StateChanged?.Invoke(state);
        }
    }
}
