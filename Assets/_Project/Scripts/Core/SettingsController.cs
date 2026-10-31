using System;
using UnityEngine;

namespace CampfireAdventure.Core
{
    /// <summary>
    /// The player's saved options: master / music / sound-effect volume, fullscreen and whether the
    /// controls hint is shown. Values are stored in PlayerPrefs and applied immediately. Master volume is
    /// applied globally through the AudioListener; the others are read by whatever plays that kind of sound.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SettingsController : MonoBehaviour
    {
        private const string MasterKey = "cfa.master", MusicKey = "cfa.music", SfxKey = "cfa.sfx", FullscreenKey = "cfa.fullscreen", HintKey = "cfa.hint";

        public float Master { get; private set; } = 1f;
        public float Music { get; private set; } = 0.8f;
        public float Sfx { get; private set; } = 1f;
        public bool Fullscreen { get; private set; } = true;
        public bool ShowControlsHint { get; private set; } = true;

        public event Action Changed;

        private void Awake()
        {
            Master = PlayerPrefs.GetFloat(MasterKey, 1f);
            Music = PlayerPrefs.GetFloat(MusicKey, 0.8f);
            Sfx = PlayerPrefs.GetFloat(SfxKey, 1f);
            Fullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
            ShowControlsHint = PlayerPrefs.GetInt(HintKey, 1) == 1;
            Apply(applyDisplay: false);
        }

        public void SetMaster(float v) { Master = Mathf.Clamp01(v); PlayerPrefs.SetFloat(MasterKey, Master); Apply(false); Changed?.Invoke(); }
        public void SetMusic(float v) { Music = Mathf.Clamp01(v); PlayerPrefs.SetFloat(MusicKey, Music); Changed?.Invoke(); }
        public void SetSfx(float v) { Sfx = Mathf.Clamp01(v); PlayerPrefs.SetFloat(SfxKey, Sfx); Changed?.Invoke(); }
        public void SetFullscreen(bool on) { Fullscreen = on; PlayerPrefs.SetInt(FullscreenKey, on ? 1 : 0); Apply(true); Changed?.Invoke(); }
        public void SetShowControlsHint(bool on) { ShowControlsHint = on; PlayerPrefs.SetInt(HintKey, on ? 1 : 0); Changed?.Invoke(); }

        private void OnDisable() => PlayerPrefs.Save();

        private void Apply(bool applyDisplay)
        {
            AudioListener.volume = Master;
#if !UNITY_EDITOR
            if (applyDisplay || Screen.fullScreen != Fullscreen) Screen.fullScreen = Fullscreen;
#endif
        }
    }
}
