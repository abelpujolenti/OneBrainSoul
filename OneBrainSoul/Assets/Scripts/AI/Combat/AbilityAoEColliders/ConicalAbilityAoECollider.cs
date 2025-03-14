using System.Collections.Generic;
using ECS.Components.AI.Combat.Abilities;
using ECS.Entities;
using UnityEngine;

namespace AI.Combat.AbilityAoEColliders
{
    public class ConicalAbilityAoECollider : AbilityAoECollider<ConicalAbilityComponent>
    {
        [SerializeField] private SphereCollider _sphereCollider;
        
        private AnimationCurve _heightResizeCurve;
        private AnimationCurve _radiusResizeCurve;

        private float _radius;

        public override void SetAbilitySpecs(Transform parentTransform, BasicAbilityComponent basicAbilityComponent, 
            ConicalAbilityComponent conicalAbilityComponent, EntityType typesAffectedByTheAbility)
        {
            base.SetAbilitySpecs(parentTransform, basicAbilityComponent, conicalAbilityComponent, typesAffectedByTheAbility);
            
            _sphereCollider.radius = conicalAbilityComponent.GetHeight();
            _radius = conicalAbilityComponent.GetRadius();
            
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
    }
}