using AI.Combat.Enemy;
using UnityEngine;

namespace AI.Combat.Contexts
{
    public class TrifaceContext : AIEnemyContext
    {
        protected TrifaceContext(uint totalHealth, float radius, float height, float sightMaximumDistance, 
            Transform agentTransform) : base(AIEnemyType.TRIFACE, totalHealth, radius, height, sightMaximumDistance, agentTransform)
        {
            
        }
    }
}