using System.Collections;
using CampfireAdventure.Campfire;
using CampfireAdventure.Core;
using CampfireAdventure.Objectives;
using CampfireAdventure.Player;
using UnityEngine;

namespace CampfireAdventure.Audio
{
    /// <summary>
    /// Plays the prototype's sound effects by reacting to gameplay events: axe hits on wood and stone,
    /// the stone breaking, footsteps while walking, logs being placed, ignition sparks, the fire catching
    /// and the fire's crackle loop. It only listens; no gameplay code calls it. All sounds are 2D because
    /// the camera is far from the action; the fire loop is attenuated by the explorer's distance instead.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameAudio : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private GameActionController actions;
        [SerializeField] private AxeSwingAnimator axe;
        [SerializeField] private CampfireController campfire;
        [SerializeField] private AutoNavigationController navigation;
        [SerializeField, Tooltip("Used to measure distance to the fire for the crackle loop.")]
        private Transform fireAnchor;
        [SerializeField, Tooltip("Optional: plays the completion chime when the last objective is done.")]
        private ObjectiveManager objectives;
        [SerializeField, Tooltip("Optional: the player's saved sound-effect volume.")]
        private SettingsController settings;

        [Header("Clips")]
        [SerializeField] private AudioClip woodChop;
        [SerializeField] private AudioClip rockHit;
        [SerializeField] private AudioClip rockBreak;
        [SerializeField] private AudioClip footstepsLoop;
        [SerializeField, Tooltip("PLACEHOLDER until a real flint/spark sound is supplied.")] private AudioClip sparkStrike;
        [SerializeField, Tooltip("PLACEHOLDER until a real ignition sound is supplied.")] private AudioClip fireWhoosh;
        [SerializeField, Tooltip("PLACEHOLDER until a real fire loop is supplied.")] private AudioClip fireLoop;
        [SerializeField, Tooltip("PLACEHOLDER until a real completion jingle is supplied.")] private AudioClip completionChime;

        [Header("Volumes (0-1)")]
        [SerializeField, Range(0f, 1f)] private float master = 1f;
        [SerializeField, Range(0f, 1f)] private float chop = 0.9f;
        [SerializeField, Range(0f, 1f)] private float stone = 0.9f;
        [SerializeField, Range(0f, 1f)] private float footsteps = 0.5f;
        [SerializeField, Range(0f, 1f)] private float logThud = 0.35f;
        [SerializeField, Range(0f, 1f)] private float sparks = 0.7f;
        [SerializeField, Range(0f, 1f)] private float whoosh = 0.8f;
        [SerializeField, Range(0f, 1f)] private float fire = 0.45f;
        [SerializeField, Range(0f, 1f)] private float chime = 0.7f;

        [Header("Footsteps")]
        [SerializeField, Min(0.1f), Tooltip("Agent speed (units/s) at which the loop plays at normal pitch.")]
        private float normalSpeed = 3.6f;

        [Header("Fire loop distance falloff")]
        [SerializeField, Min(0f)] private float fireNearDistance = 4f;
        [SerializeField, Min(1f)] private float fireFarDistance = 30f;
        [SerializeField, Range(0f, 1f)] private float fireFarVolume = 0.2f;

        private float UserSfx => settings != null ? settings.Sfx : 1f;

        private const int PoolSize = 6;
        private AudioSource[] pool;
        private int nextPool;
        private AudioSource stepSource, fireSource;
        private Coroutine sparkRoutine;

        private void Awake()
        {
            pool = new AudioSource[PoolSize];
            for (int i = 0; i < PoolSize; i++) pool[i] = NewSource("SFX_" + i, false);
            stepSource = NewSource("Footsteps", true); stepSource.clip = footstepsLoop; stepSource.volume = 0f;
            fireSource = NewSource("FireLoop", true); fireSource.clip = fireLoop; fireSource.volume = 0f;
        }

        private AudioSource NewSource(string name, bool loop)
        {
            var go = new GameObject(name); go.transform.SetParent(transform, false);
            var s = go.AddComponent<AudioSource>();
            s.playOnAwake = false; s.loop = loop; s.spatialBlend = 0f;
            return s;
        }

        private void OnEnable()
        {
            if (axe != null) axe.Impact += OnAxeImpact;
            if (campfire != null) { campfire.StateChanged += OnFireState; campfire.LogPlaced += OnLogPlaced; }
            if (objectives != null) objectives.AllCompleted += OnAllCompleted;
        }

        private void OnDisable()
        {
            if (axe != null) axe.Impact -= OnAxeImpact;
            if (campfire != null) { campfire.StateChanged -= OnFireState; campfire.LogPlaced -= OnLogPlaced; }
            if (objectives != null) objectives.AllCompleted -= OnAllCompleted;
        }

        private void OnAllCompleted() => PlayOneShot(completionChime, chime, 1f, 1f);

        private void Update()
        {
            UpdateFootsteps();
            UpdateFireLoop();
        }

        // ---------------------------------------------------------------- events

        private void OnAxeImpact(int index, int total)
        {
            var current = actions.CurrentAction;
            if (current == GameActionType.CollectWood)
            {
                PlayOneShot(woodChop, chop * (index == total - 1 ? 1f : 0.85f), 0.94f, 1.06f);
            }
            else if (current == GameActionType.CollectStones)
            {
                if (index < total - 1) PlayOneShot(rockHit, stone * 0.85f, 0.93f, 1.07f);
                else PlayOneShot(rockBreak, stone, 0.98f, 1.02f);
            }
        }

        private float lastLogThud = -10f;

        private void OnLogPlaced()
        {
            // a few soft thuds rather than one per log (there are 14 pieces landing in ~2 seconds)
            if (Time.time - lastLogThud < 0.3f) return;
            lastLogThud = Time.time;
            PlayOneShot(woodChop, logThud, 0.55f, 0.7f);
        }

        private void OnFireState(CampfireState state)
        {
            if (state == CampfireState.Igniting)
            {
                if (sparkRoutine != null) StopCoroutine(sparkRoutine);
                sparkRoutine = StartCoroutine(SparkSounds());
            }
            else
            {
                if (sparkRoutine != null) { StopCoroutine(sparkRoutine); sparkRoutine = null; }
                if (state == CampfireState.Lit) PlayOneShot(fireWhoosh, whoosh, 0.98f, 1.02f);
            }
        }

        private IEnumerator SparkSounds()
        {
            // two flint strikes while the sparks fly; the second is cut off if the fire catches
            PlayOneShot(sparkStrike, sparks, 0.97f, 1.03f);
            yield return new WaitForSeconds(0.6f);
            PlayOneShot(sparkStrike, sparks * 0.8f, 0.9f, 1.0f);
            sparkRoutine = null;
        }

        // ---------------------------------------------------------------- loops

        private void UpdateFootsteps()
        {
            if (footstepsLoop == null) return;
            float speed = navigation.CurrentSpeed;
            float target = speed > 0.4f ? footsteps * master * UserSfx : 0f;
            stepSource.volume = Mathf.MoveTowards(stepSource.volume, target, Time.deltaTime * 5f);
            stepSource.pitch = Mathf.Lerp(0.8f, 1.05f, Mathf.Clamp01(speed / normalSpeed));
            if (stepSource.volume > 0.01f && !stepSource.isPlaying) stepSource.Play();
            else if (stepSource.volume <= 0.01f && stepSource.isPlaying) stepSource.Pause();
        }

        private void UpdateFireLoop()
        {
            if (fireLoop == null || campfire == null) return;
            bool lit = campfire.State == CampfireState.Lit;
            float target = 0f;
            if (lit)
            {
                Vector3 d = navigation.transform.position - fireAnchor.position; d.y = 0f;
                float k = Mathf.InverseLerp(fireNearDistance, fireFarDistance, d.magnitude);
                target = fire * master * UserSfx * Mathf.Lerp(1f, fireFarVolume, k);
            }
            fireSource.volume = Mathf.MoveTowards(fireSource.volume, target, Time.deltaTime * (lit ? 0.6f : 2f));
            if (fireSource.volume > 0.005f && !fireSource.isPlaying) fireSource.Play();
            else if (fireSource.volume <= 0.005f && fireSource.isPlaying) fireSource.Stop();
        }

        // ---------------------------------------------------------------- helpers

        private void PlayOneShot(AudioClip clip, float volume, float pitchMin, float pitchMax)
        {
            if (clip == null) return;
            var s = pool[nextPool]; nextPool = (nextPool + 1) % PoolSize;
            s.pitch = Random.Range(pitchMin, pitchMax);
            s.PlayOneShot(clip, Mathf.Clamp01(volume * master * UserSfx));
        }
    }
}
