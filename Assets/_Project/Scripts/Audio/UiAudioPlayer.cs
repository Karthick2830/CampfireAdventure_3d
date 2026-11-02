using CampfireAdventure.Core;
using UnityEngine;

namespace CampfireAdventure.Audio
{
    /// <summary>
    /// Plays the menu sounds (hover, click, back). Its audio source ignores the listener pause, so menu sounds
    /// still work while the game is paused. <see cref="UiSound"/> components on buttons call into this.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UiAudioPlayer : MonoBehaviour
    {
        public static UiAudioPlayer Instance { get; private set; }

        [SerializeField] private SettingsController settings;
        [SerializeField, Tooltip("PLACEHOLDER")] private AudioClip hover;
        [SerializeField, Tooltip("PLACEHOLDER")] private AudioClip click;
        [SerializeField, Tooltip("PLACEHOLDER")] private AudioClip back;
        [SerializeField, Tooltip("PLACEHOLDER: played each time the exercise counts a rep.")] private AudioClip rep;
        [SerializeField, Range(0f, 1f)] private float volume = 0.6f;

        private AudioSource source;

        private void Awake()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false; source.spatialBlend = 0f; source.ignoreListenerPause = true;
        }

        private void OnEnable() => Instance = this;
        private void OnDisable() { if (Instance == this) Instance = null; }

        public void PlayHover() => Play(hover, 0.5f);
        public void PlayClick() => Play(click, 1f);
        public void PlayBack() => Play(back, 1f);
        public void PlayRep() => Play(rep, 1.2f);

        private void Play(AudioClip clip, float scale)
        {
            if (clip == null) return;
            float v = volume * scale * (settings != null ? settings.Sfx : 1f);
            source.PlayOneShot(clip, Mathf.Clamp01(v));
        }
    }
}
