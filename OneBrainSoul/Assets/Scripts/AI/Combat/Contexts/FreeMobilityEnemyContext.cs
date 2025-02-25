using ECS.Entities;
using Interfaces.AI.UBS.Enemy.FreeMobilityEnemy;
using UnityEngine;

namespace AI.Combat.Contexts
{
    public abstract class FreeMobilityEnemyContext : AIEnemyContext, IFreeMobilityEnemyPatrolUtility, 
        IFreeMobilityEnemyInvestigateAreaUtility
    {
        private bool _hasReachedDestination;
        
        protected FreeMobilityEnemyContext(EntityType entityType, uint totalHealth, uint maximumHeadYawRotation, 
            float radius, float height, float sightMaximumDistance, uint fov, Transform headAgentTransform, Transform bodyAgentTransform) : 
            base(entityType, totalHealth, maximumHeadYawRotation, radius, height, sightMaximumDistance, fov, headAgentTransform,
            bodyAgentTransform)
        {
            _hasReachedDestination = false;
        }

        public void SetHasReachedDestination(bool hasReachedDestination)
        {
            _hasReachedDestination = hasReachedDestination;
        }

        public bool HasReachedDestination()
        {
            return _hasReachedDestination;
        }
    }
}