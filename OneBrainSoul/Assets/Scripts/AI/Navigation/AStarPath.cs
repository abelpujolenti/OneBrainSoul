using System.Collections.Generic;
using AI.Combat.CombatNavigation;
using Interfaces.AI.Navigation;
using UnityEngine;

namespace AI.Navigation
{
    public class AStarPath : IPosition
    {
        public NavMeshGraph navMeshGraph;
        public IPosition position;
        public List<Node> path = new List<Node>();

        public AStarPath(IPosition position)
        {
            navMeshGraph = new NavMeshGraph();
            navMeshGraph.LoadGraph();
            this.position = position;
        }

        public Vector3 GetPosition()
        {
            return position.GetPosition();
        }
    }
}