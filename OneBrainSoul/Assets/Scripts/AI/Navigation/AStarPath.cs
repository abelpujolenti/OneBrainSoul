using System.Collections.Generic;
using AI.Combat.CombatNavigation;
using Interfaces.AI.Navigation;
using UnityEngine;

namespace AI.Navigation
{
    public class AStarPath : IPosition
    {
        private NavMeshGraph _navMeshGraph;
        public IPosition destinationPosition;
        public Vector3 deviationVector;
        public List<Node> path = new List<Node>();
        public List<DynamicObstacle> dynamicObstacles = new List<DynamicObstacle>();
        
        public Vector3 origin;
        public Vector3 destination;
        public List<Vector3> dynamicObstaclesPositions = new List<Vector3>();

        public AStarPath(IPosition destinationPosition, NavMeshGraph navMeshGraph)
        {
            _navMeshGraph = new NavMeshGraph();
            _navMeshGraph.LoadGraph();
            this.destinationPosition = destinationPosition;
        }

        public void UpdateNavMeshGraphObstacles()
        {
            for (int i = 0; i < dynamicObstaclesPositions.Count; i++)
            {
                _navMeshGraph.UpdateEdgeWeights(dynamicObstaclesPositions[i], dynamicObstacles[i].radius, 100);
            }
        }

        public Vector3 GetPosition()
        {
            return destinationPosition.GetPosition();
        }

        public NavMeshGraph GetNavMeshGraph()
        {
            return _navMeshGraph;
        }

        public Vector3 lastGoal;
    }
}