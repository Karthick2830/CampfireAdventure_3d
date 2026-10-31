using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace CampfireAdventure.Player
{
    /// <summary>
    /// Sends the player's NavMeshAgent to a destination, validates the path first and reports
    /// arrival or failure. It knows nothing about game actions or input.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class AutoNavigationController : MonoBehaviour
    {
        [Header("Arrival")]
        [SerializeField, Min(0f), Tooltip("Extra distance (on top of the agent's stopping distance) that still counts as arrived.")]
        private float arrivalPadding = 0.1f;

        [SerializeField, Min(0.1f), Tooltip("How far from the requested point a valid NavMesh position may be found.")]
        private float destinationSampleRadius = 1.5f;

        [Header("Safety")]
        [SerializeField, Min(1f), Tooltip("Navigation fails if it takes longer than this many seconds.")]
        private float maxTravelSeconds = 90f;

        [SerializeField, Min(0.5f), Tooltip("Navigation fails if the agent makes no progress for this many seconds.")]
        private float stuckSeconds = 3f;

        [Header("Route preference")]
        [SerializeField, Range(0, 31), Tooltip("NavMesh area index used by the trail and camp/yard/quarry floors (set on their NavMeshModifier).")]
        private int trailAreaIndex = 3;

        [SerializeField, Min(1f), Tooltip("Cost multiplier for open ground (area 0). Higher keeps the explorer on the trails.")]
        private float openGroundCost = 6f;

        [SerializeField, Min(1f), Tooltip("Cost multiplier for trail and camp floors.")]
        private float trailCost = 1f;

        [Header("Facing")]
        [SerializeField, Min(30f), Tooltip("Degrees per second used when turning to face a target after arriving.")]
        private float turnSpeed = 540f;

        private NavMeshAgent agent;
        private Vector3 currentTarget;
        private Coroutine monitor;
        private bool navigating;

        /// <summary>Raised once when the agent reaches its destination.</summary>
        public event Action Arrived;

        /// <summary>Raised once with a short player-facing reason when navigation cannot finish.</summary>
        public event Action<string> Failed;

        public bool IsNavigating => navigating;

        /// <summary>Current planar speed of the agent in units per second.</summary>
        public float CurrentSpeed => agent != null && agent.enabled && agent.isOnNavMesh ? agent.velocity.magnitude : 0f;

        public float DistanceTo(Vector3 worldPoint)
        {
            Vector3 delta = worldPoint - transform.position; delta.y = 0f;
            return delta.magnitude;
        }

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        // The agent is saved disabled and switched on here, after every scene object (including the baked
        // NavMesh) has finished waking up. Enabling it earlier made a built game log
        // "Failed to create agent because there is no valid NavMesh" once at startup.
        private void Start()
        {
            if (!agent.enabled) agent.enabled = true;
            ApplyAreaCosts();
        }

        // Per-agent costs (no project-wide NavMesh settings are changed): prefer trails over open ground.
        // Re-applied on every trip because NavMeshAgent.Warp resets them.
        private void ApplyAreaCosts()
        {
            agent.SetAreaCost(0, openGroundCost);
            agent.SetAreaCost(trailAreaIndex, trailCost);
        }

        /// <summary>
        /// Validates the destination and starts moving. Returns false (with a reason) when the
        /// NavMesh is missing, the destination is off the mesh, or no complete path exists.
        /// </summary>
        public bool TryStartNavigation(Vector3 destination, out string failureReason)
        {
            failureReason = null;
            if (agent == null) agent = GetComponent<NavMeshAgent>();

            if (!agent.enabled || !agent.isOnNavMesh)
            {
                failureReason = "Navigation is unavailable: the explorer is not on the NavMesh.";
                return false;
            }
            if (!NavMesh.SamplePosition(destination, out NavMeshHit hit, destinationSampleRadius, NavMesh.AllAreas))
            {
                failureReason = "That destination is not reachable (no walkable ground there).";
                return false;
            }
            ApplyAreaCosts();
            var path = new NavMeshPath();
            if (!agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete)
            {
                failureReason = "That destination is not reachable (the path is blocked).";
                return false;
            }

            StopMonitor();
            currentTarget = hit.position;
            agent.updateRotation = true;
            agent.isStopped = false;
            agent.SetPath(path);
            navigating = true;
            monitor = StartCoroutine(Monitor());
            return true;
        }

        /// <summary>Stops moving immediately without raising Arrived or Failed.</summary>
        public void Cancel()
        {
            StopMonitor();
            navigating = false;
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.ResetPath();
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
        }

        /// <summary>Smoothly rotates around Y until the player looks at the target.</summary>
        public IEnumerator FaceTarget(Transform target)
        {
            if (target == null) yield break;
            agent.updateRotation = false;
            while (true)
            {
                Vector3 dir = target.position - transform.position; dir.y = 0f;
                if (dir.sqrMagnitude < 0.0001f) break;
                Quaternion want = Quaternion.LookRotation(dir.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, want, turnSpeed * Time.deltaTime);
                if (Quaternion.Angle(transform.rotation, want) < 1f) break;
                yield return null;
            }
            agent.updateRotation = true;
        }

        private IEnumerator Monitor()
        {
            float startTime = Time.time;
            float stillTime = 0f;

            while (true)
            {
                yield return null;
                if (agent.pathPending) continue;

                // remainingDistance can report Infinity on long, cost-weighted paths, so fall back to
                // the straight distance to the (NavMesh-snapped) destination.
                float remaining = agent.remainingDistance;
                if (float.IsInfinity(remaining) || float.IsNaN(remaining)) remaining = DistanceTo(currentTarget);
                if (remaining <= agent.stoppingDistance + arrivalPadding)
                {
                    FinishNavigation();
                    Arrived?.Invoke();
                    yield break;
                }

                // "Stuck" means the agent has actually stopped moving short of the goal.
                stillTime = agent.velocity.sqrMagnitude < 0.01f ? stillTime + Time.deltaTime : 0f;
                if (stillTime > stuckSeconds)
                {
                    FinishNavigation();
                    Failed?.Invoke("The explorer got stuck on the way.");
                    yield break;
                }
                if (Time.time - startTime > maxTravelSeconds)
                {
                    FinishNavigation();
                    Failed?.Invoke("The trip took too long and was cancelled.");
                    yield break;
                }
            }
        }

        private void FinishNavigation()
        {
            navigating = false;
            monitor = null;
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.ResetPath();
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
        }

        private void StopMonitor()
        {
            if (monitor != null) StopCoroutine(monitor);
            monitor = null;
        }
    }
}
