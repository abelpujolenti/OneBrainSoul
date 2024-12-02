using System;
using System.Collections.Generic;
using AI.Combat.CombatNavigation;
using AI.Navigation;
using ECS.Components.AI.Navigation;
using ECS.Entities.AI.Combat;
using UnityEngine;
using UnityEngine.AI;

namespace ECS.Systems.AI.Navigation
{
    public class UpdateAgentDestinationSystem
    {
        public Action UpdateAgentDestination(NavMeshAgentComponent navMeshAgentComponent, Vector3 origin, Vector3 destination, 
            AStarPath aStarPath)
        {
            List<Node> customPath = AStarPathFindingAlgorithm.FindPath(aStarPath.navMeshGraph, origin, destination);
            
            aStarPath.navMeshGraph.ResetNodesImportantInfo();

            return () =>
            {
                if (customPath.Count == 0)
                {
                    Debug.Log("Return");
                    return;
                }
                
                Debug.Log("Continue");

                NavMeshPath navMeshPath = new NavMeshPath();

                for (int i = 0; i < customPath.Count; i++)
                {
                    navMeshAgentComponent.GetNavMeshAgent().CalculatePath(customPath[i].position, navMeshPath);
                }

                navMeshAgentComponent.GetNavMeshAgent().SetPath(navMeshPath);
            };
        }
    }
}
