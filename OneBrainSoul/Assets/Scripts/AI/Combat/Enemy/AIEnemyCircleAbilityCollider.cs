using AI.Combat.AbilityColliders;
using ECS.Components.AI.Combat;
using UnityEngine;

namespace AI.Combat.Enemy
{
    public class AIEnemyCircleAbilityCollider : AIEnemyAbilityCollider
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

        public override void SetAbilityTargets(int targetsLayerMask)
        {
            _sphereCollider.includeLayers = targetsLayerMask;
            _sphereCollider.excludeLayers = ~targetsLayerMask;
        }

        public void SetCircleAbilityComponent(CircleAttackComponent circleAttackComponent)
        {
            _circleAttackComponent = circleAttackComponent;
            float radius = _circleAttackComponent.GetRadius();
            float height = _circleAttackComponent.GetHeight();
            
            _sphereCollider.radius = radius;
        }
    }
}