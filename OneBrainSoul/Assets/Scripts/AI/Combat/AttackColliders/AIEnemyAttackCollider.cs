using System.Collections.Generic;
using ECS.Entities.AI.Combat;
using UnityEngine;

namespace AI.Combat.AttackColliders
{
    public abstract class AIEnemyAttackCollider : AIAttackCollider
    {
        protected bool _isWarning;
        
        protected List<AIAlly> _combatAgentsTriggering = new List<AIAlly>();

        protected abstract Vector2[] GetCornerPoints();

        public bool HasCombatAgentsTriggering()
        {
            return _combatAgentsTriggering.Count != 0;
        }

        protected override void OnDisable()
        {
            _combatAgentsTriggering.Clear();
        }
    }
}