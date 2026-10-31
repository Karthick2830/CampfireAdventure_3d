using System;
using System.Collections.Generic;
using CampfireAdventure.Campfire;
using CampfireAdventure.CameraSystem;
using CampfireAdventure.Gathering;
using CampfireAdventure.Objectives;
using CampfireAdventure.Player;
using UnityEngine;
using UnityEngine.AI;

namespace CampfireAdventure.Core
{
    /// <summary>
    /// Owns "start the adventure again". Restart cancels whatever the explorer is doing, puts the
    /// explorer back at the start, and resets the inventory and collection points, the campfire, the
    /// time of day and the objectives. It holds no gameplay rules of its own.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AdventureSession : MonoBehaviour
    {
        [Header("Systems to reset")]
        [SerializeField] private GameActionController actions;
        [SerializeField] private ResourceCollector collector;
        [SerializeField] private CampfireController campfire;
        [SerializeField] private TimeOfDayDirector timeOfDay;
        [SerializeField] private ObjectiveManager objectives;

        [Header("Explorer")]
        [SerializeField] private NavMeshAgent explorerAgent;
        [SerializeField] private AutoNavigationController explorerNavigation;
        [SerializeField, Tooltip("Where the explorer stands at the start of the adventure.")] private Transform startPoint;
        [SerializeField] private IsometricCameraController cameraController;

        [Header("Input")]
        [SerializeField, Tooltip("Components implementing ISessionInputSource (e.g. KeyboardActionSource).")]
        private List<MonoBehaviour> inputSources = new List<MonoBehaviour>();

        /// <summary>Raised after a restart has finished.</summary>
        public event Action Restarted;

        private void OnEnable()
        {
            foreach (var mb in inputSources)
            {
                if (mb is ISessionInputSource s) s.RestartRequested += RestartAdventure;
                else if (mb != null) Debug.LogError($"{mb.name} does not implement ISessionInputSource.", this);
            }
        }

        private void OnDisable()
        {
            foreach (var mb in inputSources)
                if (mb is ISessionInputSource s) s.RestartRequested -= RestartAdventure;
        }

        public void RestartAdventure()
        {
            actions.Cancel();
            explorerNavigation.Cancel();

            collector.ResetAll();     // inventory, collection points, pile visuals
            campfire.ResetFire();     // unlit, no logs, no flames, attempts back to 0
            timeOfDay.ResetTime();    // back to the evening look

            explorerAgent.Warp(startPoint.position);
            explorerAgent.transform.rotation = Quaternion.identity;
            if (cameraController != null) cameraController.SnapToTarget();

            objectives.ResetAll();    // last, so listeners see a clean world
            Restarted?.Invoke();
        }
    }
}
