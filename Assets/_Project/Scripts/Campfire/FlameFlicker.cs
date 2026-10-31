using UnityEngine;

namespace CampfireAdventure.Campfire
{
    /// <summary>
    /// Animates the low-poly flame meshes (scale/sway) and the fire light while the fire burns.
    /// <see cref="Intensity"/> (0..1) fades the whole effect in and out.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FlameFlicker : MonoBehaviour
    {
        [SerializeField, Tooltip("Flame meshes to animate.")]
        private Transform[] flames;

        [SerializeField] private Light fireLight;

        [SerializeField, Min(0f), Tooltip("Light intensity at full strength.")]
        private float lightIntensity = 9f;

        [SerializeField, Min(0f)] private float flickerSpeed = 3.2f;
        [SerializeField, Range(0f, 1f)] private float flickerAmount = 0.35f;
        [SerializeField, Min(0f), Tooltip("Degrees per second each flame sways.")]
        private float swayDegrees = 6f;

        private Vector3[] baseScale;
        private float[] phase;
        private float intensity;

        public float Intensity
        {
            get => intensity;
            set { intensity = Mathf.Clamp01(value); Apply(Time.time); }
        }

        private void Awake()
        {
            baseScale = new Vector3[flames.Length];
            phase = new float[flames.Length];
            for (int i = 0; i < flames.Length; i++)
            {
                baseScale[i] = flames[i].localScale;
                phase[i] = i * 7.31f;
            }
        }

        private void Update() => Apply(Time.time);

        private void Apply(float time)
        {
            if (baseScale == null) return;
            for (int i = 0; i < flames.Length; i++)
            {
                float n = Mathf.PerlinNoise(time * flickerSpeed + phase[i], phase[i] * 0.37f);
                float h = 1f + (n - 0.5f) * 2f * flickerAmount;
                float w = 1f - (n - 0.5f) * flickerAmount;
                flames[i].localScale = new Vector3(baseScale[i].x * w, baseScale[i].y * h, baseScale[i].z * w) * intensity;
                flames[i].localRotation = Quaternion.Euler(0f, 0f, (n - 0.5f) * swayDegrees * 2f) * Quaternion.Euler(0f, time * 25f + phase[i] * 20f, 0f);
            }
            if (fireLight != null)
            {
                float n = Mathf.PerlinNoise(time * flickerSpeed * 1.7f, 11.3f);
                fireLight.intensity = lightIntensity * intensity * (1f - flickerAmount * 0.5f + n * flickerAmount);
            }
        }
    }
}
