using System.Collections.Generic;
using AI.Combat.AbilitySpecs;
using AI.Combat.Enemy;
using AI.Combat.Enemy.Triface;
using Interfaces.AI.UBS.Enemy;
using Interfaces.AI.UBS.Enemy.Triface;
using UnityEngine;

namespace AI.Combat.Contexts
{
    public class TrifaceContext : AIEnemyContext, IEnemyPatrolUtility, IEnemyLookForNewTargetUtility, 
        IEnemyGetCloserToTargetUtility, ITrifaceSlamUtility
    {
        AbilityCast _slamCast;
        
        public TrifaceContext(uint totalHealth, float radius, float height, float sightMaximumDistance,
            Transform agentTransform, AbilityCast slamCast) : base(EnemyType.TRIFACE, totalHealth, radius, 
            height, sightMaximumDistance, agentTransform)
        {
            _repeatableActions = new List<uint>
            {
                (uint)TrifaceAction.PATROL,
                (uint)TrifaceAction.ROTATE
            };

            _slamCast = slamCast;
        }

        public float GetMinimumRangeToCast()
        {
            return _slamCast.minimumRangeToCast;
        }

        public float GetMaximumRangeToCast()
        {
            return _slamCast.maximumRangeToCast;
        }

        public bool IsOnCooldown()
        {
            return _slamCast.IsOnCooldown();
        }
    }
}