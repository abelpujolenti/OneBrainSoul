﻿namespace AI.Combat.CombatNavigation
{
    public class Edge
    {
        public uint fromNodeIndex;
        public uint toNodeIndex;
        public bool isAJump;
        public float distance;
        public float cost;
        public float defaultCost;
        public float baseCostMultiplier;

        public void ResetCost()
        {
            cost = defaultCost;
        }

        public void ResetDefaultCost()
        {
            cost = distance;
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