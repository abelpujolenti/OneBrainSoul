using System;
using AI.Combat.CombatNavigation;
using AI.Navigation;
using Interfaces.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace ECS.Components.AI.Navigation
{
    [Serializable]
    public class NavMeshAgentComponent
    {
        private NavMeshAgentSpecs _navMeshAgentSpecs;
        private TransformComponent _transformComponent;
        private NavMeshAgent _agent;
        private AStarPath _aStarPath;

        public NavMeshAgentComponent(NavMeshAgentSpecs navMeshAgentSpecs, NavMeshAgent agent, Transform transform)
        {
            _navMeshAgentSpecs = navMeshAgentSpecs;
            _agent = agent;
            _transformComponent = new TransformComponent(transform);
            _aStarPath = new AStarPath(_transformComponent);
        }

        public NavMeshAgent GetNavMeshAgent()
        {
            return _agent;
        }

        public TransformComponent GetTransformComponent()
        {
            return _transformComponent;
        }

        public AStarPath GetAStarPath()
        {
            return _aStarPath;
        }

        public void SetAStarPathDestination(IPosition iPosition)
        {
            _aStarPath.destinationPosition = iPosition;
        }
    }
}
