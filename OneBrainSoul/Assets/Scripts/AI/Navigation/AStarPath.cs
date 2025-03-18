using System;
using System.Collections.Generic;
using AI.Combat.CombatNavigation;
using Interfaces.AI.Navigation;
using Managers;
using UnityEngine;

namespace AI.Navigation
{
    public class AStarPath : IPosition
    {
        public NavMeshGraph navMeshGraph;
        public IPosition destinationPosition;
        public Vector3 deviationVector;
        public List<Node> path = new List<Node>();

        private Action<Vector3, float> _onUpdateDynamicObstacle;
        
        public Vector3 origin;
        public Vector3 destination;

        private Action<bool> _hasReachDestinationAction;

        public AStarPath(IPosition destinationPosition, NavMeshGraph navMeshGraph)
        {
            this.destinationPosition = destinationPosition;
            this.navMeshGraph = navMeshGraph;
        }

        public void UpdateNavMeshGraphObstacles()
        {
            List<DynamicObstacleThreadSafe> dynamicObstacleAndRadius = EventsManager.OnUpdateDynamicObstacle();

            foreach (DynamicObstacleThreadSafe positionAndRadius in dynamicObstacleAndRadius)
            {
                navMeshGraph.UpdateEdgeWeights(positionAndRadius.GetPosition(), positionAndRadius.GetRadius(), 100);
            }
        }

        public Vector3 GetPosition()
        {
            return destinationPosition.GetPosition();
        }

        public void SetOnReachDestination(Action<bool> hasReachDestinationAction)
        {
            _hasReachDestinationAction = hasReachDestinationAction;
        }

        public void OnSetNewDestination()
        {
            _hasReachDestinationAction(false);
        }

        public void OnReachDestination()
        {
            _hasReachDestinationAction(true);
        }
    }
}