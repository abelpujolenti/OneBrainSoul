using AI.Combat.AbilitySpecs;
using AI.Combat.ScriptableObjects;
using UnityEngine;

namespace ECS.Components.AI.Combat.Abilities
{
    public class ConicalAbilityComponent : AbilityComponent
    {
        private Vector3 _direction;
        private float _height;
        private float _radius;

        public ConicalAbilityComponent(AgentAbility agentAbility) : base(agentAbility)
        {
            AbilityAoE abilityAoE = agentAbility.abilityAoE;
            
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