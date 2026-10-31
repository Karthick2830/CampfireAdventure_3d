using UnityEngine;

namespace CampfireAdventure.Player
{
    /// <summary>
    /// Kicks up a small puff of dust at each footfall while the explorer moves. A footfall is detected
    /// from the foot bones coming down to the height they have when standing, so the puffs follow the
    /// actual walk/run animation. The dust colour matches the ground the foot lands on.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FootstepDust : MonoBehaviour
    {
        [SerializeField, Tooltip("Humanoid Animator (for the foot bones).")] private Animator animator;
        [SerializeField] private AutoNavigationController navigation;
        [SerializeField, Tooltip("World-space particle system used only through Emit().")] private ParticleSystem dust;

        [SerializeField, Min(0f), Tooltip("No dust below this speed.")] private float minSpeed = 0.8f;
        [SerializeField, Min(0.005f), Tooltip("A foot counts as planted when it is this close to its standing height.")]
        private float plantedTolerance = 0.05f;
        [SerializeField, Min(0.005f), Tooltip("The foot must rise this far above standing height before it can plant again.")]
        private float liftedTolerance = 0.10f;
        [SerializeField, Min(1)] private int particlesPerStep = 4;
        [SerializeField] private LayerMask groundMask = ~0;

        private Transform leftFoot, rightFoot;
        private float leftRest, rightRest;
        private bool leftPlanted = true, rightPlanted = true;

        private void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        }

        private void Start()
        {
            // The explorer spawns standing still, so the bones' current height is their resting height.
            leftRest = HeightOf(leftFoot);
            rightRest = HeightOf(rightFoot);
        }

        private float HeightOf(Transform foot) => foot.position.y - transform.position.y;

        private void LateUpdate()
        {
            bool moving = navigation.CurrentSpeed > minSpeed;
            Track(leftFoot, leftRest, ref leftPlanted, moving);
            Track(rightFoot, rightRest, ref rightPlanted, moving);
        }

        private void Track(Transform foot, float rest, ref bool planted, bool moving)
        {
            float h = HeightOf(foot) - rest;
            if (!moving) { planted = h < liftedTolerance; return; } // never burst when starting to walk

            if (planted)
            {
                if (h > liftedTolerance) planted = false;
            }
            else if (h < plantedTolerance)
            {
                planted = true;
                Puff(foot.position);
            }
        }

        private void Puff(Vector3 footPosition)
        {
            Vector3 ground = new Vector3(footPosition.x, transform.position.y + 0.04f, footPosition.z);
            Color color = new Color(0.55f, 0.58f, 0.38f, 0.7f); // grass
            if (Physics.Raycast(footPosition + Vector3.up * 0.6f, Vector3.down, out RaycastHit hit, 2f, groundMask, QueryTriggerInteraction.Ignore))
            {
                ground.y = hit.point.y + 0.03f;
                string n = hit.collider.name;
                if (n.StartsWith("Quarry")) color = new Color(0.74f, 0.74f, 0.76f, 0.8f);
                else if (n.StartsWith("Lumberyard")) color = new Color(0.62f, 0.45f, 0.29f, 0.8f);
                else if (n.StartsWith("Campsite") || n.StartsWith("Trail")) color = new Color(0.80f, 0.65f, 0.45f, 0.8f);
            }

            Vector3 back = -transform.forward;
            for (int i = 0; i < particlesPerStep; i++)
            {
                var p = new ParticleSystem.EmitParams
                {
                    position = ground + Random.insideUnitSphere * 0.06f,
                    velocity = back * Random.Range(0.1f, 0.5f) + Vector3.up * Random.Range(0.25f, 0.6f) + Random.insideUnitSphere * 0.12f,
                    startColor = color,
                    startSize = Random.Range(0.32f, 0.6f),
                    startLifetime = Random.Range(0.6f, 1.0f),
                };
                dust.Emit(p, 1);
            }
        }
    }
}
