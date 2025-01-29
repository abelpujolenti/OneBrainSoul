using AI.Combat.AbilitySpecs;
using AI.Combat.ScriptableObjects;
using UnityEngine;

namespace ECS.Components.AI.Combat.Abilities
{
    public class ConeAbilityComponent : AbilityComponent
    {
        private Vector3 _direction;
        private float _radius;
        private float _degrees;

        public ConeAbilityComponent(AgentAbility agentAbility) : base(agentAbility)
        {
            AbilityAoE abilityAoE = agentAbility.abilityAoE;
            
            _direction = abilityAoE.direction;
            _radius = abilityAoE.radius;
            _degrees = abilityAoE.degrees;
        }

        public Vector3 GetDirection()
        {
            return _direction;
        }

        public float GetRadius()
        {
            return _radius;
        }

        public float GetDegrees()
        {
            return _degrees;
        }
    }
}