using ECS.Components.AI.Combat.Abilities;
using UnityEngine;

namespace AI.Combat.AbilityAoEColliders
{
    public class SphericalAbilityAoECollider : AbilityAoECollider<SphericalAbilityComponent>
    {
        [SerializeField] private SphereCollider _sphereCollider;

        private AnimationCurve _radiusResizeCurve;

        public override void SetAbilitySpecs(Transform parentTransform, SphericalAbilityComponent sphericalAbilityComponent)
        {
            base.SetAbilitySpecs(parentTransform, sphericalAbilityComponent);

            _sphereCollider.radius = sphericalAbilityComponent.GetRadius();

            if (!sphericalAbilityComponent.GetAoE().doesRadiusChangeOverTheTime)
            {
                return;
            }
            
            _radiusResizeCurve = sphericalAbilityComponent.GetAoE().radiusChangeOverTime;
            _actionResizing = time =>
            {
                _sphereCollider.radius = ReturnSizeOverTime(time, _radiusResizeCurve);
            };
        }

        public override void SetAbilityTargets(int targetsLayerMask)
        {
            _sphereCollider.includeLayers = targetsLayerMask;
            _sphereCollider.excludeLayers = ~targetsLayerMask;
        }
    }
}