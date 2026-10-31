using CampfireAdventure.Core;
using UnityEngine;

namespace CampfireAdventure.Audio
{
    /// <summary>
    /// Background sound: a daytime ambience that crossfades into a night ambience as the light turns to
    /// night, plus a quiet music bed. Ambience follows the player's effects volume and the music follows
    /// the music volume. Everything loops; nothing here reacts to gameplay events.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AmbientAudio : MonoBehaviour
    {
        [SerializeField, Tooltip("Gives the evening-to-night blend (0 evening, 1 night).")] private LightingMoodController mood;
        [SerializeField] private SettingsController settings;

        [Header("Clips (all PLACEHOLDER until real ones are supplied)")]
        [SerializeField] private AudioClip dayAmbience;
        [SerializeField] private AudioClip nightAmbience;
        [SerializeField] private AudioClip music;

        [Header("Levels (0-1)")]
        [SerializeField, Range(0f, 1f)] private float ambienceLevel = 0.5f;
        [SerializeField, Range(0f, 1f)] private float musicLevel = 0.35f;
        [SerializeField, Min(0.1f)] private float fadeSpeed = 1.5f;

        private AudioSource day, night, bed;

        private void Awake()
        {
            day = Make("AmbienceDay", dayAmbience);
            night = Make("AmbienceNight", nightAmbience);
            bed = Make("Music", music);
        }

        private AudioSource Make(string name, AudioClip clip)
        {
            var go = new GameObject(name); go.transform.SetParent(transform, false);
            var s = go.AddComponent<AudioSource>();
            s.clip = clip; s.loop = true; s.playOnAwake = false; s.spatialBlend = 0f; s.volume = 0f;
            if (clip != null) s.Play();
            return s;
        }

        private void Update()
        {
            float sfx = settings != null ? settings.Sfx : 1f, mus = settings != null ? settings.Music : 1f;
            float blend = mood != null ? mood.Blend : 0f;
            Approach(day, ambienceLevel * sfx * (1f - blend));
            Approach(night, ambienceLevel * sfx * blend);
            Approach(bed, musicLevel * mus);
        }

        private void Approach(AudioSource s, float target)
        {
            s.volume = Mathf.MoveTowards(s.volume, target, fadeSpeed * Time.unscaledDeltaTime);
        }
    }
}
