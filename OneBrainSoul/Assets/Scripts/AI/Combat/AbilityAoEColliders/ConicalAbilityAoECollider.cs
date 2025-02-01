using ECS.Components.AI.Combat.Abilities;
using UnityEngine;

namespace AI.Combat.AbilityAoEColliders
{
    public class ConicalAbilityAoECollider : AbilityAoECollider<ConicalAbilityComponent>
    {
        [SerializeField] private SphereCollider _sphereCollider;
        
        private Quaternion _parentRotation;

        private Vector3 _direction;
        
        private AnimationCurve _heightResizeCurve;
        private AnimationCurve _radiusResizeCurve;

        private float _radius;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            _parentRotation = _parentTransform.rotation;
            
            Rotate();
        }

        public override void SetAbilitySpecs(Transform parentTransform, ConicalAbilityComponent conicalAbilityComponent)
        {
            base.SetAbilitySpecs(parentTransform, conicalAbilityComponent);
            
            _sphereCollider.radius = conicalAbilityComponent.GetHeight();
            _radius = conicalAbilityComponent.GetRadius();

            _direction = conicalAbilityComponent.GetDirection().normalized;
            
            if (conicalAbilityComponent.GetAoE().doesHeightChangeOverTheTime)
            {
                _heightResizeCurve = conicalAbilityComponent.GetAoE().heightChangeOverTime;
                _actionResizing = time =>
                {
                    _sphereCollider.radius = ReturnSizeOverTime(time, _heightResizeCurve);
                };
            }

            if (!conicalAbilityComponent.GetAoE().doesRadiusChangeOverTheTime)
            {
                return;
            }
            
            _radiusResizeCurve = conicalAbilityComponent.GetAoE().radiusChangeOverTime;

            if (conicalAbilityComponent.GetAoE().doesHeightChangeOverTheTime)
            {
                _actionResizing = time =>
                {
                    _radius = ReturnSizeOverTime(time, _radiusResizeCurve);
                };
                return;
            }
            
            _actionResizing += time =>
            {
                _radius = ReturnSizeOverTime(time, _radiusResizeCurve);
            };
        }

        public override void SetAbilityTargets(int targetsLayerMask)
        {
            _sphereCollider.includeLayers = targetsLayerMask;
            _sphereCollider.excludeLayers = ~targetsLayerMask;
        }

        private void Rotate()
        {
            transform.rotation = _parentRotation * Quaternion.LookRotation(_direction, Vector3.up);
        }
    }
}