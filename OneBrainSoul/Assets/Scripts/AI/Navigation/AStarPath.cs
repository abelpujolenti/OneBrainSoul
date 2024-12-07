using System.Collections.Generic;
using AI.Combat.CombatNavigation;
using Interfaces.AI.Navigation;
using UnityEngine;

namespace AI.Navigation
{
    public class AStarPath : IPosition
    {
        public NavMeshGraph navMeshGraph;
        public IPosition destinationPosition;
        public List<Node> path = new List<Node>();

        public AStarPath(IPosition destinationPosition, NavMeshGraph navMeshGraph)
        {
            this.navMeshGraph = new NavMeshGraph();
            this.navMeshGraph.LoadGraph();
            this.destinationPosition = destinationPosition;
        }

        public Vector3 GetPosition()
        {
            return destinationPosition.GetPosition();
        }
    }
}