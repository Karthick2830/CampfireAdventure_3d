using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CampfireAdventure.Core
{
    /// <summary>
    /// Blends the scene from its saved look (a golden-hour evening) to a moonlit night.
    /// <see cref="Blend"/> is 0 for evening and 1 for night. The scene's own values are captured on
    /// Awake, so the evening look is simply whatever the scene is saved with.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LightingMoodController : MonoBehaviour
    {
        [Header("Scene references")]
        [SerializeField, Tooltip("The scene's directional light (sun at evening, moon at night).")]
        private Light sun;
        [SerializeField] private Camera targetCamera;
        [SerializeField, Tooltip("Global post-processing volume (its profile is cloned at runtime).")]
        private Volume volume;

        [Header("Night look")]
        [SerializeField] private Color nightLightColor = new Color(0.45f, 0.58f, 1f);
        [SerializeField, Min(0f)] private float nightLightIntensity = 0.55f;
        [SerializeField, Tooltip("Euler angles of the moon light at full night.")]
        private Vector3 nightLightEuler = new Vector3(52f, -50f, 0f);
        [SerializeField] private Color nightAmbientSky = new Color(0.10f, 0.14f, 0.28f);
        [SerializeField] private Color nightAmbientEquator = new Color(0.09f, 0.11f, 0.20f);
        [SerializeField] private Color nightAmbientGround = new Color(0.04f, 0.05f, 0.09f);
        [SerializeField] private Color nightBackground = new Color(0.03f, 0.05f, 0.10f);
        [SerializeField] private Color nightColorFilter = new Color(0.75f, 0.85f, 1f);
        [SerializeField] private float nightExposure = -0.1f;
        [SerializeField, Range(0f, 1f)] private float nightVignette = 0.38f;

        [Header("Warm lights that come on at night (lanterns etc.)")]
        [SerializeField] private Light[] nightLights = new Light[0];

        private Color baseSunColor, baseSky, baseEquator, baseGround, baseBackground, baseFilter;
        private float baseSunIntensity, baseExposure, baseVignette;
        private Quaternion baseSunRotation;
        private float[] nightLightMax;
        private ColorAdjustments colorAdjust;
        private Vignette vignette;
        private Coroutine fade;

        /// <summary>0 = evening, 1 = night.</summary>
        public float Blend { get; private set; }

        private void Awake()
        {
            if (sun == null) sun = RenderSettings.sun;
            if (targetCamera == null) targetCamera = Camera.main;
            baseSunColor = sun.color; baseSunIntensity = sun.intensity; baseSunRotation = sun.transform.rotation;
            baseSky = RenderSettings.ambientSkyColor; baseEquator = RenderSettings.ambientEquatorColor; baseGround = RenderSettings.ambientGroundColor;
            baseBackground = targetCamera.backgroundColor;

            if (volume != null && volume.profile != null) // .profile clones the shared asset so play mode never edits it
            {
                volume.profile.TryGet(out colorAdjust);
                volume.profile.TryGet(out vignette);
                if (colorAdjust != null) { baseFilter = colorAdjust.colorFilter.value; baseExposure = colorAdjust.postExposure.value; }
                if (vignette != null) baseVignette = vignette.intensity.value;
            }

            nightLightMax = new float[nightLights.Length];
            for (int i = 0; i < nightLights.Length; i++)
            {
                if (nightLights[i] == null) continue;
                nightLightMax[i] = nightLights[i].intensity;
                nightLights[i].intensity = 0f; nightLights[i].enabled = false;
            }
        }

        private void OnDestroy()
        {
            if (sun != null && targetCamera != null) Apply(0f); // leave the scene in its saved look
        }

        /// <summary>Sets the blend immediately.</summary>
        public void SetBlend(float blend)
        {
            if (fade != null) { StopCoroutine(fade); fade = null; }
            Apply(Mathf.Clamp01(blend));
        }

        /// <summary>Smoothly moves the blend to <paramref name="target"/> over <paramref name="seconds"/>.</summary>
        public void FadeTo(float target, float seconds)
        {
            if (fade != null) StopCoroutine(fade);
            fade = StartCoroutine(FadeRoutine(Mathf.Clamp01(target), Mathf.Max(0.01f, seconds)));
        }

        private IEnumerator FadeRoutine(float target, float seconds)
        {
            float start = Blend;
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                Apply(Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t / seconds)));
                yield return null;
            }
            Apply(target);
            fade = null;
        }

        private void Apply(float b)
        {
            Blend = b;
            sun.color = Color.Lerp(baseSunColor, nightLightColor, b);
            sun.intensity = Mathf.Lerp(baseSunIntensity, nightLightIntensity, b);
            sun.transform.rotation = Quaternion.Slerp(baseSunRotation, Quaternion.Euler(nightLightEuler), b);
            RenderSettings.ambientSkyColor = Color.Lerp(baseSky, nightAmbientSky, b);
            RenderSettings.ambientEquatorColor = Color.Lerp(baseEquator, nightAmbientEquator, b);
            RenderSettings.ambientGroundColor = Color.Lerp(baseGround, nightAmbientGround, b);
            targetCamera.backgroundColor = Color.Lerp(baseBackground, nightBackground, b);
            if (colorAdjust != null)
            {
                colorAdjust.colorFilter.value = Color.Lerp(baseFilter, nightColorFilter, b);
                colorAdjust.postExposure.value = Mathf.Lerp(baseExposure, nightExposure, b);
            }
            if (vignette != null) vignette.intensity.value = Mathf.Lerp(baseVignette, nightVignette, b);
            for (int i = 0; i < nightLights.Length; i++)
            {
                if (nightLights[i] == null) continue;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.35f, 0.9f, b));
                nightLights[i].intensity = nightLightMax[i] * k;
                nightLights[i].enabled = k > 0.001f;
            }
        }
    }
}
