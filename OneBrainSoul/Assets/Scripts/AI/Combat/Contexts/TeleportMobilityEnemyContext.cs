using ECS.Entities;
using UnityEngine;

namespace AI.Combat.Contexts
{
    public abstract class TeleportMobilityEnemyContext : AIEnemyContext
    {
        private bool _hasReachedDestination;
        
        protected TeleportMobilityEnemyContext(EntityType entityType, uint totalHealth, uint maximumHeadYawRotation, 
            float radius, float height, float sightMaximumDistance, uint fov, Transform headAgentTransform, Transform bodyAgentTransform) : 
            base(entityType, totalHealth, maximumHeadYawRotation,radius, height, sightMaximumDistance, 
            fov, headAgentTransform, bodyAgentTransform)
        {
            
        }
    }
}