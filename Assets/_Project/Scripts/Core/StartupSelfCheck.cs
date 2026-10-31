using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace CampfireAdventure.Core
{
    /// <summary>
    /// A one-time check a moment after the scene starts: is the baked NavMesh loaded, and is the explorer
    /// standing on it? If the agent could not register at load time (it can happen in a build when the agent
    /// wakes before the NavMesh data), this re-creates it and says so in the log. The result is always logged
    /// so a build's player log shows at a glance that navigation is healthy.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class StartupSelfCheck : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent explorer;
        [SerializeField] private Transform startPoint;

        private IEnumerator Start()
        {
            yield return null; // let every component finish waking up
            yield return null;

            int triangles = NavMesh.CalculateTriangulation().indices.Length / 3;
            bool onMesh = explorer.isOnNavMesh;

            if (!onMesh && triangles > 0 && NavMesh.SamplePosition(startPoint.position, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                explorer.enabled = false;               // re-create the agent now that the NavMesh exists
                explorer.transform.position = hit.position;
                explorer.enabled = true;
                onMesh = explorer.isOnNavMesh;
                Debug.LogWarning("[SelfCheck] The explorer was not on the NavMesh at start; re-created the agent. On NavMesh now: " + onMesh);
            }

            if (triangles == 0 || !onMesh)
                Debug.LogError("[SelfCheck] Navigation is NOT working: NavMesh triangles = " + triangles + ", explorer on NavMesh = " + onMesh);
            else
                Debug.Log("[SelfCheck] Navigation OK: NavMesh triangles = " + triangles + ", explorer on NavMesh = true");
        }
    }
}
