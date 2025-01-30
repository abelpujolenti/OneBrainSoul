using System.Collections.Generic;
using UnityEngine;

namespace AI.Combat.CombatNavigation
{
    public class Node
    {
        public uint index;
        public Vector3 position;
        public int heightIndex;
        public List<Edge> edges = new List<Edge>();
        public float gCost = Mathf.Infinity;
        public float hCost = 0;
        public float fCost => gCost + hCost;
        public Node parent;
    }
}