using AI.Combat.AttackColliders;
using ECS.Components.AI.Combat;
using ECS.Entities.AI.Combat;
using UnityEngine;

namespace AI.Combat.Ally
{
    public class AIAllyCircleAttackCollider : AIAllyAttackCollider
    {
        [SerializeField] private SphereCollider _sphereCollider;
        
        private AllyCircleAttackComponent _circleAttackComponent;

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
            _combatAgentsTriggering.Clear();
        }

        public override void SetAttackTargets(int targetsLayerMask)
        {
            _sphereCollider.includeLayers = targetsLayerMask;
            _sphereCollider.excludeLayers = ~targetsLayerMask;
        }

        public void SetCircleAttackComponent(AllyCircleAttackComponent circleAttackComponent)
        {
            _ownerID = circleAttackComponent.GetAllyID();
            
            _circleAttackComponent = circleAttackComponent;
            _sphereCollider.radius = _circleAttackComponent.GetRadius();
        }

        public override void StartInflictingDamage()
        {
            foreach (AIEnemy enemy in _combatAgentsTriggering)
            {
                InflictDamageToEnemy(enemy);

                if (enemy.GetAgentID() != _ownerContext.GetRivalID())
                {
                    continue;
                }
                
                enemy.RequestDuel(_ownerID, _ownerContext);
            }
        }

        private void InflictDamageToEnemy(AIEnemy enemy)
        {
            enemy.OnReceiveDamage(new AllyDamageComponent(_circleAttackComponent.GetDamage(),
                _circleAttackComponent.GetStressDamage()));
        }

        private void OnTriggerEnter(Collider other)
        {
            AIEnemy targetEnemy = other.GetComponent<AIEnemy>();

            if (_combatAgentsTriggering.Contains(targetEnemy))
            {
                return;
            }
            
            _combatAgentsTriggering.Add(targetEnemy);
        }
    }
}