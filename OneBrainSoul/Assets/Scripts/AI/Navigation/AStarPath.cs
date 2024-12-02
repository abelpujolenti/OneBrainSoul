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
        public Node goalNode;
        public List<Node> previousPathGCosts;

        public AStarPath(NavMeshGraph navMeshGraph, IPosition position)
        {
            this.navMeshGraph = navMeshGraph;
            this.position = position;
        }

        public Vector3 GetPosition()
        {
            return position.GetPosition();
        }
    }
}