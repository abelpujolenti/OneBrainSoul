using System.Collections.Generic;
using AI.Combat.AttackColliders;
using ECS.Entities.AI.Combat;
using Unity.AI.Navigation;
using UnityEngine;

namespace AI.Combat
{
    public abstract class AIEnemyAttackCollider : AIAttackCollider
    {
        protected bool _isWarning;

        [SerializeField] protected List<NavMeshModifierVolume> _navMeshModifierVolumes = 
            new List<NavMeshModifierVolume>();
        
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