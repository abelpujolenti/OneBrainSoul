using AI.Combat.AbilitySpecs;
using AI.Combat.ScriptableObjects;
using UnityEngine;

namespace ECS.Components.AI.Combat.Abilities
{
    public class RectangleAbilityComponent : AbilityComponent
    {
        private Vector3 _direction;
        private float _length;
        private float _wideness;

        public RectangleAbilityComponent(AgentAbility agentAbility) : base(agentAbility)
        {
            AbilityAoE abilityAoE = agentAbility.abilityAoE;
            
            _direction = abilityAoE.direction;
            _length = abilityAoE.length;
            _wideness = abilityAoE.width;
        }

        public Vector3 GetDirection()
        {
            return _direction;
        }

        public float GetLength()
        {
            return _length;
        }

        public float GetWidth()
        {
            return _wideness;
        }
    }
}