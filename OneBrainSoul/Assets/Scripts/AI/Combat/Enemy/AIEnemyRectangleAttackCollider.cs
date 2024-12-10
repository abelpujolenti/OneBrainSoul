using System;
using System.Collections.Generic;
using AI.Combat.AttackColliders;
using ECS.Components.AI.Combat;
using ECS.Entities.AI.Combat;
using Managers;
using UnityEngine;

namespace AI.Combat.Enemy
{
    public class AIEnemyRectangleAttackCollider : AIEnemyAttackCollider
    {
        [SerializeField] private BoxCollider _boxCollider;
        
        private RectangleAttackComponent _rectangleAttackComponent;

        protected override void OnEnable()
        {
            if (_rectangleAttackComponent == null)
            {
                return;
            }
            
            _stopwatch.Reset();
            _stopwatch.Start();

            _isWarning = true;
            
            MoveToPosition(_rectangleAttackComponent.GetRelativePosition());
            Rotate();

            if (_rectangleAttackComponent.IsAttachedToAttacker())
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
                ally.FreeOfWarnArea(_rectangleAttackComponent, this);
            }
            base.OnDisable();
        }

        private void Rotate()
        {
            transform.rotation = 
                _parentRotation * Quaternion.LookRotation(_rectangleAttackComponent.GetDirection().normalized, Vector3.up);
        }

        public override void SetAttackTargets(int targetsLayerMask)
        {
            _boxCollider.includeLayers = targetsLayerMask;
            _boxCollider.excludeLayers = ~targetsLayerMask;
        }

        public void SetRectangleAttackComponent(RectangleAttackComponent rectangleAttackComponent)
        {
            _rectangleAttackComponent = rectangleAttackComponent;

            float height = _rectangleAttackComponent.GetHeight();
            float width = _rectangleAttackComponent.GetWidth();
            float length = _rectangleAttackComponent.GetLength();
            
            _boxCollider.size = new Vector3(width, height, length);

            Vector3 center = new Vector3
            {
                x = Convert.ToInt16(!_rectangleAttackComponent.IsRelativePositionXCenterOfColliderX()) * (width / 2),
                y = Convert.ToInt16(!_rectangleAttackComponent.IsRelativePositionYCenterOfColliderY()) * (height / 2),
                z = Convert.ToInt16(!_rectangleAttackComponent.IsRelativePositionZCenterOfColliderZ()) * (length / 2)
            };

            _boxCollider.center = center;
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
            ally.FreeOfWarnArea(_rectangleAttackComponent, this);
            ally.OnReceiveDamage(new DamageComponent(_rectangleAttackComponent.GetDamage()));
        }

        private void OnTriggerEnter(Collider other)
        {
            AIAlly targetAlly = other.GetComponent<AIAlly>();

            if (_isWarning)
            {
                targetAlly.WarnOncomingDamage(_rectangleAttackComponent, this, _stopwatch.ElapsedMilliseconds / 1000);    
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
            
            targetAlly.FreeOfWarnArea(_rectangleAttackComponent, this);    
            _combatAgentsIDsTriggering.Remove(targetAlly.GetAgentID());
        }
    }
}