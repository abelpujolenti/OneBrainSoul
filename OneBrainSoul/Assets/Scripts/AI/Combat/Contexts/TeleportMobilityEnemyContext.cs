using ECS.Entities;
using UnityEngine;

namespace AI.Combat.Contexts
{
    public abstract class TeleportMobilityEnemyContext : AIEnemyContext
    {
        private bool _hasReachedDestination;
        
        protected TeleportMobilityEnemyContext(EntityType entityType, uint totalHealth, float radius, float height, 
            Transform headAgentTransform, Transform bodyAgentTransform) : base(entityType, totalHealth,radius, height, 
            headAgentTransform, bodyAgentTransform)
        {
            
        }
    }
}