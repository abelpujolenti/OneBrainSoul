using System.Collections.Generic;
using UnityEngine;

namespace AI.Combat.CombatNavigation
{
    public class Node
    {
        public Vector3 position;
        public List<Edge> neighbors = new List<Edge>();
    }
}