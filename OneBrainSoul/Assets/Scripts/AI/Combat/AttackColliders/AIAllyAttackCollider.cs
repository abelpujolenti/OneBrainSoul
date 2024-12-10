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

        protected override void RemoveAgentID(uint agentID)
        {
            if (_ownerID == agentID)
            {
                Destroy(gameObject);
                return;
            }
            
            for (int i = 0; i < _combatAgentsTriggering.Count; i++)
            {
                if (_combatAgentsTriggering[i].GetAgentID() != agentID)
                {
                    continue;
                }
                
                _combatAgentsTriggering.RemoveAt(i);
            }
        }

        protected override void OnDisable()
        {
            _combatAgentsTriggering.Clear();
        }
    }
}