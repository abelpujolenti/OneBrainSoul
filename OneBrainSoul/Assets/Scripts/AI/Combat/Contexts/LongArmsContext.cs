using AI.Combat.Enemy;
using UnityEngine;

namespace AI.Combat.Contexts
{
    public class LongArmsContext : AIEnemyContext
    {
        public LongArmsContext(uint totalHealth, float radius, float height, float sightMaximumDistance,
            Transform agentTransform) : base(EnemyType.LONG_ARMS, totalHealth, radius, height, sightMaximumDistance, agentTransform)
        {
            
        }
    }
}