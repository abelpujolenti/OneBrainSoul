using ECS.Components.AI.Combat.Abilities;
using UnityEngine;

namespace AI.Combat.AbilityAoEColliders
{
    public class SphericalAbilityAoECollider : AbilityAoECollider<SphericalAbilityComponent>
    {
        [SerializeField] private SphereCollider _sphereCollider;

        private AnimationCurve _radiusResizeCurve;

        public override void SetAbilitySpecs(Transform parentTransform, BasicAbilityComponent basicAbilityComponent, 
            SphericalAbilityComponent sphericalAbilityComponent)
        {
            base.SetAbilitySpecs(parentTransform, basicAbilityComponent, sphericalAbilityComponent);

            _sphereCollider.radius = sphericalAbilityComponent.GetRadius();

            if (!sphericalAbilityComponent.GetAoE().doesRadiusChangeOverTheTime)
            {
                return;
            }
            
            _radiusResizeCurve = sphericalAbilityComponent.GetAoE().radiusChangeOverTime;
            _actionResizing = time =>
            {
                float newRadius = ReturnSizeOverTime(time, _radiusResizeCurve);
                
                _sphereCollider.radius = newRadius;

                float newScale = newRadius / _radiusResizeCurve.keys[0].value;
                
                _childWithParticleSystem.transform.localScale = new Vector3(newScale, newScale, newScale);
            };
        }
    }
}