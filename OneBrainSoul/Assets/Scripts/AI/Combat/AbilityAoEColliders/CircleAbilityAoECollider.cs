using ECS.Components.AI.Combat.Abilities;
using UnityEngine;

namespace AI.Combat.AbilityAoEColliders
{
    public class CircleAbilityAoECollider : AbilityAoECollider<CircleAbilityComponent>
    {
        [SerializeField] private SphereCollider _sphereCollider;

        protected override void OnEnable()
        {
            _stopwatch.Start();
            
            MoveToPosition(_relativePosition);

            _actionAttaching();
        }

        protected override void OnDisable()
        {
            //_stopwatch.Reset();
        }

        public override void SetAbilitySpecs(CircleAbilityComponent circleAbilityComponent)
        {
            base.SetAbilitySpecs(circleAbilityComponent);

            float radius = circleAbilityComponent.GetRadius();
            float height = circleAbilityComponent.GetHeight();
            
            _sphereCollider.radius = radius;
        }

        public override void SetAbilityTargets(int targetsLayerMask)
        {
            _sphereCollider.includeLayers = targetsLayerMask;
            _sphereCollider.excludeLayers = ~targetsLayerMask;
        }
    }
}