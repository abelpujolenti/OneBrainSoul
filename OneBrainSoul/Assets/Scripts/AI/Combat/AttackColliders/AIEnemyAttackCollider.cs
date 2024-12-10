using System.Collections.Generic;
using System.Diagnostics;

namespace AI.Combat.AttackColliders
{
    public abstract class AIEnemyAttackCollider : AIAttackCollider
    {
        private uint _ownerID;
        
        protected bool _isWarning;
        
        protected List<uint> _combatAgentsIDsTriggering = new List<uint>();

        protected Stopwatch _stopwatch = new Stopwatch();

        public void SetOwner(uint ownerID)
        {
            _ownerID = ownerID;
        }

        public bool HasCombatAgentsTriggering()
        {
            return _combatAgentsIDsTriggering.Count != 0;
        }

        protected override void RemoveAgentID(uint agentID)
        {
            if (_ownerID == agentID)
            {
                Destroy(gameObject);
                return;
            }
            
            for (int i = 0; i < _combatAgentsIDsTriggering.Count; i++)
            {
                if (_combatAgentsIDsTriggering[i] != agentID)
                {
                    continue;
                }
                
                _combatAgentsIDsTriggering.RemoveAt(i);
            }
        }

        protected override void OnDisable()
        {
            _combatAgentsIDsTriggering.Clear();
        }
    }
}