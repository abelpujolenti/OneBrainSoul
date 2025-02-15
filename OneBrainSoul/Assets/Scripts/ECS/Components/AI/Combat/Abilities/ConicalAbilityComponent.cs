using AI.Combat.AbilitySpecs;
using AI.Combat.ScriptableObjects;
using UnityEngine;

namespace ECS.Components.AI.Combat.Abilities
{
    public class ConicalAbilityComponent : AreaAbilityComponent
    {
        private Vector3 _direction;
        private float _height;
        private float _radius;

        public ConicalAbilityComponent(AreaAbilityProperties areaBasicAbilityProperties) : base(areaBasicAbilityProperties)
        {
            AbilityAoE abilityAoE = areaBasicAbilityProperties.abilityAoE;
            
            _direction = abilityAoE.direction;
            _height = abilityAoE.height;
            _radius = abilityAoE.radius;
        }

        public Vector3 GetDirection()
        {
            return _direction;
        }

        public float GetHeight()
        {
            return _height;
        }

        public float GetRadius()
        {
            return _radius;
        }
    }
}