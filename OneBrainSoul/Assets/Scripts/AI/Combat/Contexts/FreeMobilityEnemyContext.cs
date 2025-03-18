using ECS.Entities;
using Interfaces.AI.UBS.Enemy.FreeMobilityEnemy;
using UnityEngine;

namespace AI.Combat.Contexts
{
    public abstract class FreeMobilityEnemyContext : AIEnemyContext, IFreeMobilityEnemyRotateInSituUtility, 
        IFreeMobilityEnemyPatrolUtility, IFreeMobilityEnemyInvestigateAreaUtility
    {
        private bool _hasStopped;
        
        private bool _hasReachedDestination;
        
        protected FreeMobilityEnemyContext(EntityType entityType, uint totalHealth, float radius, float height, 
            Transform headAgentTransform, Transform bodyAgentTransform) : base(entityType, totalHealth, radius, 
            height, headAgentTransform, bodyAgentTransform)
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

        public void SetHasStopped(bool hasStopped)
        {
            _hasStopped = hasStopped;
        }

        public bool HasStopped()
        {
            return _hasStopped;
        }
    }
}