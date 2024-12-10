using System.Collections.Generic;
using AI.Combat.ScriptableObjects;
using ECS.Entities.AI.Combat;

namespace AI.Combat.AttackColliders
{
    public abstract class AIAllyAttackCollider : AIAttackCollider
    {
        protected uint _ownerID;

        protected AIAllyContext _ownerContext;
        
        protected List<AIEnemy> _combatAgentsTriggering = new List<AIEnemy>();

        public void SetOwner(uint ownerID, AIAllyContext ownerContext)
        {
            _ownerID = ownerID;
            _ownerContext = ownerContext;
        }

        protected override void OnDisable()
        {
            _combatAgentsTriggering.Clear();
        }
    }
}