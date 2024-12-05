using UnityEngine;

namespace AI.Combat.CombatNavigation
{
    public class Edge
    {
        public Node fromNode;
        public Node toNode;
        public float cost;
        public float defaultCost;
        public float baseCostMultiplier;

        public void ResetCost()
        {
            cost = defaultCost;
        }

        public void ResetDefaultCost()
        {
            cost = Vector3.Distance(fromNode.position, toNode.position);
        }

        public void MultiplyCost(float multiplierValue)
        {
            cost *= baseCostMultiplier * multiplierValue;
        }

        public void MultiplyDefaultCost(float multiplierValue)
        {
            defaultCost *= baseCostMultiplier * multiplierValue;
        }
    }
}