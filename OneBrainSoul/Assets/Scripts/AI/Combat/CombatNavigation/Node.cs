using System.Collections.Generic;
using Interfaces.AI.Navigation;
using UnityEngine;

namespace AI.Combat.CombatNavigation
{
    public class Node : ICopy<Node>
    {
        public uint index;
        public Vector3 position;
        public List<Edge> edges = new List<Edge>();
        public float gCost = Mathf.Infinity;
        public float hCost = 0;
        public float fCost => gCost + hCost;
        public Node parent;
        
        
        public Node Copy()
        {
            List<Edge> copyEdges = new List<Edge>();

            foreach (Edge edge in edges)
            {
                copyEdges.Add(edge.Copy());
            }
            
            return new Node
            {
                index = index,
                position = position,
                edges = copyEdges,
                gCost = Mathf.Infinity,
                hCost = 0
            };
        }
    }
}