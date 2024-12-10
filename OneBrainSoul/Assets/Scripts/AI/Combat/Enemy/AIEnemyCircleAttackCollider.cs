using System.Collections.Generic;
using AI.Combat.AttackColliders;
using ECS.Components.AI.Combat;
using ECS.Entities.AI.Combat;
using Managers;
using UnityEngine;

namespace AI.Combat.Enemy
{
    public class AIEnemyCircleAttackCollider : AIEnemyAttackCollider
    {
        [SerializeField] private SphereCollider _sphereCollider;
        
        private CircleAttackComponent _circleAttackComponent;

        protected override void OnEnable()
        {
            if (_circleAttackComponent == null)
            {
                return;
            }
            
            _stopwatch.Reset();
            _stopwatch.Start();
            
            MoveToPosition(_circleAttackComponent.GetRelativePosition());

            if (!_circleAttackComponent.IsAttachedToAttacker())
            {
                return;
            }

            transform.parent = null;
        }

        protected override void OnDisable()
        {
            List<AIAlly> combatAgentsIDsTriggering = new List<AIAlly>();

            foreach (uint agentID in _combatAgentsIDsTriggering)
            {
                combatAgentsIDsTriggering.Add(CombatManager.Instance.RequestAlly(agentID));
            }
            
            foreach (AIAlly ally in combatAgentsIDsTriggering)
            {
                ally.FreeOfWarnArea(_circleAttackComponent, this);
            }
            base.OnDisable();
        }

        public override void SetAttackTargets(int targetsLayerMask)
        {
            _sphereCollider.includeLayers = targetsLayerMask;
            _sphereCollider.excludeLayers = ~targetsLayerMask;
        }

        public void SetCircleAttackComponent(CircleAttackComponent circleAttackComponent)
        {
            _circleAttackComponent = circleAttackComponent;
            float radius = _circleAttackComponent.GetRadius();
            float height = _circleAttackComponent.GetHeight();
            
            _sphereCollider.radius = radius;
        }

        public override void StartInflictingDamage()
        {
            _isWarning = false;

            List<AIAlly> combatAgentsIDsTriggering = new List<AIAlly>();

            foreach (uint agentID in _combatAgentsIDsTriggering)
            {
                combatAgentsIDsTriggering.Add(CombatManager.Instance.RequestAlly(agentID));
            }

            foreach (AIAlly ally in combatAgentsIDsTriggering)
            {
                InflictDamageToAlly(ally);
            }
        }

        private void InflictDamageToAlly(AIAlly ally)
        {
            ally.OnReceiveDamage(new DamageComponent(_circleAttackComponent.GetDamage()));
        }

        private void OnTriggerEnter(Collider other)
        {
            AIAlly targetAlly = other.GetComponent<AIAlly>();

            if (_isWarning)
            {
                targetAlly.WarnOncomingDamage(_circleAttackComponent, this, _stopwatch.ElapsedMilliseconds / 1000);    
            }
            else
            {
                InflictDamageToAlly(targetAlly);   
            }
            
            _combatAgentsIDsTriggering.Add(targetAlly.GetAgentID());
        }

        private void OnTriggerExit(Collider other)
        {
            AIAlly targetAlly = other.GetComponent<AIAlly>();

            if (!_isWarning)
            {
                return;
            }
            
            targetAlly.FreeOfWarnArea(_circleAttackComponent, this);    
            _combatAgentsIDsTriggering.Remove(targetAlly.GetAgentID());
        }
    }
}