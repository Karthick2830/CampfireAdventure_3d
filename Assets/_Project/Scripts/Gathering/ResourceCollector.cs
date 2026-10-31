using System;
using CampfireAdventure.Core;
using CampfireAdventure.Player;
using UnityEngine;

namespace CampfireAdventure.Gathering
{
    /// <summary>
    /// Applies the gameplay effect of collecting and placing resources. For a collect action it picks the
    /// nearest collection point that still has units (as the action's destination), and when
    /// <see cref="GameActionController.ActionPerformed"/> fires (once per action) it takes one unit from that
    /// point and adds it to the inventory. It also refuses actions that make no sense yet.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ResourceCollector : MonoBehaviour, IActionValidator, IActionTargetProvider
    {
        [SerializeField] private GameActionController actions;
        [SerializeField] private ResourceInventory inventory;
        [SerializeField] private ResourceCollectionPoint[] woodPoints = new ResourceCollectionPoint[0];
        [SerializeField] private ResourceCollectionPoint[] stonePoints = new ResourceCollectionPoint[0];
        [SerializeField, Tooltip("Axe swing that drives the wood- and stone-chopping hit effects.")]
        private AxeSwingAnimator axe;

        /// <summary>Short player-facing messages (e.g. "Collected wood 1/2").</summary>
        public event Action<string> Notice;

        private ResourceCollectionPoint target; // the point the current collect action is aimed at

        private void OnEnable()
        {
            actions.ActionPerformed += OnPerformed;
            if (axe != null) axe.Impact += OnAxeImpact;
        }

        private void OnDisable()
        {
            actions.ActionPerformed -= OnPerformed;
            if (axe != null) axe.Impact -= OnAxeImpact;
        }

        public bool CanStart(GameActionType action, out string reason)
        {
            reason = null;
            switch (action)
            {
                case GameActionType.CollectWood:
                    if (inventory.IsFull(ResourceType.Wood)) { reason = $"You already have all the wood you need ({inventory.Wood}/{inventory.RequiredWood})."; return false; }
                    if (!AnyLeft(woodPoints)) { reason = "There is no wood left to collect."; return false; }
                    return true;
                case GameActionType.CollectStones:
                    if (inventory.IsFull(ResourceType.Stone)) { reason = $"You already have all the stones you need ({inventory.Stones}/{inventory.RequiredStones})."; return false; }
                    if (!AnyLeft(stonePoints)) { reason = "There are no stones left to break."; return false; }
                    return true;
                case GameActionType.PrepareCampfire:
                    if (!inventory.HasEnoughForCampfire) { reason = $"Collect the wood and stones first (wood {inventory.Wood}/{inventory.RequiredWood}, stones {inventory.Stones}/{inventory.RequiredStones})."; return false; }
                    return true;
                default:
                    return true;
            }
        }

        public bool TryResolve(GameActionType action, Vector3 from, out Transform destination, out Transform lookTarget)
        {
            destination = null; lookTarget = null;
            ResourceCollectionPoint[] points = action == GameActionType.CollectWood ? woodPoints : action == GameActionType.CollectStones ? stonePoints : null;
            if (points == null) return false;

            target = Nearest(points, from);
            if (target == null) return false;
            destination = target.InteractionPoint;
            lookTarget = target.LookTarget;
            return destination != null;
        }

        private void OnPerformed(GameActionType action)
        {
            switch (action)
            {
                case GameActionType.CollectWood:
                    Collect(ResourceType.Wood, woodPoints, "Collected wood");
                    break;
                case GameActionType.CollectStones:
                    Collect(ResourceType.Stone, stonePoints, "Broke off stones");
                    break;
                case GameActionType.PrepareCampfire:
                    inventory.TrySpendForCampfire();
                    break;
            }
        }

        private void Collect(ResourceType type, ResourceCollectionPoint[] points, string message)
        {
            var point = target != null && target.HasUnits && target.Type == type ? target : Nearest(points, actions.transform.position);
            target = null;
            if (point == null) return;

            inventory.Add(type, point.Take());
            Notice?.Invoke($"{message} {inventory.Get(type)}/{inventory.Required(type)}");
        }

        private void OnAxeImpact(int index, int total)
        {
            var cur = actions.CurrentAction;
            if ((cur == GameActionType.CollectWood || cur == GameActionType.CollectStones) && target != null && target.Pile != null)
                target.Pile.PlayImpact(index, total);
        }

        private static bool AnyLeft(ResourceCollectionPoint[] points)
        {
            foreach (var p in points) if (p != null && p.HasUnits) return true;
            return false;
        }

        private static ResourceCollectionPoint Nearest(ResourceCollectionPoint[] points, Vector3 from)
        {
            ResourceCollectionPoint best = null; float bestSqr = float.MaxValue;
            foreach (var p in points)
            {
                if (p == null || !p.HasUnits || p.InteractionPoint == null) continue;
                Vector3 d = p.InteractionPoint.position - from; d.y = 0f;
                if (d.sqrMagnitude < bestSqr - 0.01f) { bestSqr = d.sqrMagnitude; best = p; }
            }
            return best;
        }

        /// <summary>Puts every collection point and the inventory back to their starting state (used by restart).</summary>
        public void ResetAll()
        {
            inventory.Clear();
            foreach (var p in woodPoints) if (p != null) p.ResetPoint();
            foreach (var p in stonePoints) if (p != null) p.ResetPoint();
            target = null;
        }
    }
}
