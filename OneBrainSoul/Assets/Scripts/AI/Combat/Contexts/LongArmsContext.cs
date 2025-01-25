using AI.Combat.Enemy;
using UnityEngine;

namespace AI.Combat.Contexts
{
    public class LongArmsContext : AIEnemyContext
    {
        protected LongArmsContext(uint totalHealth, float radius, float height, float sightMaximumDistance,
            Transform agentTransform) : base(AIEnemyType.LONG_ARMS, totalHealth, radius, height, sightMaximumDistance, agentTransform)
        {
            
        }
    }
}