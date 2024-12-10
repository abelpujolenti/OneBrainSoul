using AI.Combat.AttackColliders;
using ECS.Components.AI.Combat;
using ECS.Entities.AI.Combat;
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
            
            MoveToPosition(_circleAttackComponent.GetRelativePosition());

            if (!_circleAttackComponent.IsAttachedToAttacker())
            {
                return;
            }

            transform.parent = null;
        }

        protected override void OnDisable()
        {
            foreach (AIAlly ally in _combatAgentsTriggering)
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

            foreach (AIAlly ally in _combatAgentsTriggering)
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
                targetAlly.WarnOncomingDamage(_circleAttackComponent, this);    
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
            
            targetAlly.FreeOfWarnArea(_circleAttackComponent, this);    
            _combatAgentsTriggering.Remove(targetAlly);
        }
    }
}