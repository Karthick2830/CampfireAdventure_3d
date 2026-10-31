using System;
using CampfireAdventure.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CampfireAdventure.UI
{
    /// <summary>
    /// Settings screen: master / music / sound-effect volume sliders, fullscreen and controls-hint toggles.
    /// Changes apply and save immediately. The Back button (or Esc) raises <see cref="BackRequested"/>.
    /// </summary>
    public sealed class SettingsPanelView : UiScreen
    {
        [SerializeField] private SettingsController settings;
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private TMP_Text masterValue;
        [SerializeField] private TMP_Text musicValue;
        [SerializeField] private TMP_Text sfxValue;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Toggle hintToggle;
        [SerializeField] private Button backButton;

        public event Action BackRequested;

        protected override void Awake()
        {
            base.Awake();
            masterSlider.onValueChanged.AddListener(v => settings.SetMaster(v));
            musicSlider.onValueChanged.AddListener(v => settings.SetMusic(v));
            sfxSlider.onValueChanged.AddListener(v => settings.SetSfx(v));
            fullscreenToggle.onValueChanged.AddListener(on => settings.SetFullscreen(on));
            hintToggle.onValueChanged.AddListener(on => settings.SetShowControlsHint(on));
            backButton.onClick.AddListener(() => BackRequested?.Invoke());
        }

        private void OnEnable()
        {
            settings.Changed += Refresh;
        }

        private void OnDisable() => settings.Changed -= Refresh;

        private void Start() => Pull();

        public override void Show()
        {
            Pull();
            base.Show();
        }

        private void Refresh()
        {
            masterValue.text = Percent(settings.Master);
            musicValue.text = Percent(settings.Music);
            sfxValue.text = Percent(settings.Sfx);
        }

        // set the controls from the saved values without echoing the change back into the settings
        private void Pull()
        {
            masterSlider.SetValueWithoutNotify(settings.Master);
            musicSlider.SetValueWithoutNotify(settings.Music);
            sfxSlider.SetValueWithoutNotify(settings.Sfx);
            fullscreenToggle.SetIsOnWithoutNotify(settings.Fullscreen);
            hintToggle.SetIsOnWithoutNotify(settings.ShowControlsHint);
            Refresh();
        }

        private static string Percent(float v) => Mathf.RoundToInt(v * 100f) + "%";
    }
}
