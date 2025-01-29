using ECS.Components.AI.Combat.Abilities;
using UnityEngine;

namespace AI.Combat.AbilityAoEColliders
{
    public class ConeAbilityAoECollider : AbilityAoECollider<ConeAbilityComponent>
    {
        [SerializeField] private SphereCollider _sphereCollider;

        protected Vector3 _direction;

        protected override void OnEnable()
        {
            _stopwatch.Start();
            
            MoveToPosition(_relativePosition);
            Rotate();

            _actionAttaching();
        }

        protected override void OnDisable()
        {
            //_stopwatch.Reset();
        }

        public override void SetAbilitySpecs(ConeAbilityComponent coneAbilityComponent)
        {
            base.SetAbilitySpecs(coneAbilityComponent);

            _sphereCollider.radius = coneAbilityComponent.GetRadius();

            _direction = coneAbilityComponent.GetDirection().normalized;
            
            Rotate();
        }

        public override void SetAbilityTargets(int targetsLayerMask)
        {
            _sphereCollider.includeLayers = targetsLayerMask;
            _sphereCollider.excludeLayers = ~targetsLayerMask;
        }

        private void Rotate()
        {
            transform.rotation = _parentRotation * 
                    Quaternion.LookRotation(_direction, Vector3.up);
        }
    }
}