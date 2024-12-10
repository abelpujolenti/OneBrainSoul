using System.Collections.Generic;
using AI.Combat.AttackColliders;
using ECS.Components.AI.Combat;
using ECS.Entities.AI.Combat;
using Managers;
using UnityEngine;

namespace AI.Combat.Enemy
{
    public class AIEnemyConeAttackCollider : AIEnemyAttackCollider
    {
        private ConeAttackComponent _coneAttackComponent;

        private SphereCollider _sphereCollider;

        protected override void OnEnable()
        {
            if (_coneAttackComponent == null)
            {
                return;
            }
            
            _stopwatch.Reset();
            _stopwatch.Start();
            
            MoveToPosition(_coneAttackComponent.GetRelativePosition());
            Rotate();

            if (!_coneAttackComponent.IsAttachedToAttacker())
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
                ally.FreeOfWarnArea(_coneAttackComponent, this);
            }
            base.OnDisable();
        }

        private void Rotate()
        {
            transform.rotation = 
                _parentRotation * Quaternion.LookRotation(_coneAttackComponent.GetDirection().normalized, Vector3.up);
        }

        public override void SetAttackTargets(int targetsLayerMask)
        {
            _sphereCollider.includeLayers = targetsLayerMask;
            _sphereCollider.excludeLayers = ~targetsLayerMask;
        }

        public void SetConeAttackComponent(ConeAttackComponent coneAttackComponent)
        {
            _sphereCollider = gameObject.AddComponent<SphereCollider>();
            _sphereCollider.isTrigger = true;
            
            _coneAttackComponent = coneAttackComponent;
            
            Rotate();
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
            ally.OnReceiveDamage(new DamageComponent(_coneAttackComponent.GetDamage()));
        }

        private void OnTriggerEnter(Collider other)
        {
            AIAlly targetAlly = other.GetComponent<AIAlly>();

            if (_isWarning)
            {
                targetAlly.WarnOncomingDamage(_coneAttackComponent, this, _stopwatch.ElapsedMilliseconds / 1000);    
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
            
            targetAlly.FreeOfWarnArea(_coneAttackComponent, this);    
            _combatAgentsIDsTriggering.Remove(targetAlly.GetAgentID());
        }
    }
}