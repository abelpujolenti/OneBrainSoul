using System.Collections.Generic;
using AI.Combat.CombatNavigation;
using ECS.Components.AI.Navigation;
using ECS.Entities.AI.Combat;
using Interfaces.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace ECS.Systems.AI.Navigation
{
    public class UpdateAgentDestinationSystem
    {
        private AStarPathFindingAlgorithm _aStarPathFindingAlgorithm = new AStarPathFindingAlgorithm();

        public NavMeshGraph navMeshGraph = new NavMeshGraph();

        public void UpdateAgentDestination(NavMeshAgentComponent navMeshAgentComponent, IPosition positionComponent)
        {
            Vector3 destination = positionComponent.GetPosition();
            
            List<Vector3> customPath = _aStarPathFindingAlgorithm.FindPath(navMeshGraph, 
                navMeshAgentComponent.GetTransformComponent().GetPosition(), destination);

            NavMeshPath navMeshPath = new NavMeshPath();

            if (customPath.Count == 0)
            {
                return;
            }

            for (int i = 0; i < customPath.Count; i++)
            {
                navMeshAgentComponent.GetNavMeshAgent().CalculatePath(customPath[i], navMeshPath);
            }

            navMeshAgentComponent.GetNavMeshAgent().SetPath(navMeshPath);
        }
    }
}
