using AI.Combat.AttackColliders;
using ECS.Components.AI.Combat;
using ECS.Entities.AI.Combat;
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
            foreach (AIAlly ally in _combatAgentsTriggering)
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

            foreach (AIAlly ally in _combatAgentsTriggering)
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
                targetAlly.WarnOncomingDamage(_coneAttackComponent, this);    
            }
            else
            {
                InflictDamageToAlly(targetAlly);   
            }
            
            _combatAgentsTriggering.Add(targetAlly);
        }

        private void OnTriggerExit(Collider other)
        {
            AIAlly targetAlly = other.GetComponent<AIAlly>();

            if (!_isWarning)
            {
                return;
            }
            
            targetAlly.FreeOfWarnArea(_coneAttackComponent, this);    
            _combatAgentsTriggering.Remove(targetAlly);
        }
    }
}