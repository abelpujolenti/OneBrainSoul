using AI.Combat.AbilitySpecs;
using AI.Combat.ScriptableObjects;
using UnityEngine;

namespace ECS.Components.AI.Combat.Abilities
{
    public class RectangularAbilityComponent : AbilityComponent
    {
        private Vector3 _direction;
        private float _heigth;
        private float _width;
        private float _length;

        public RectangularAbilityComponent(AgentAbility agentAbility) : base(agentAbility)
        {
            AbilityAoE abilityAoE = agentAbility.abilityAoE;
            
            _direction = abilityAoE.direction;
            _heigth = abilityAoE.height;
            _width = abilityAoE.width;
            _length = abilityAoE.length;
        }

        public Vector3 GetDirection()
        {
            return _direction;
        }

        public float GetHeight()
        {
            return _heigth;
        }

        public float GetWidth()
        {
            return _width;
        }

        public float GetLength()
        {
            return _length;
        }
    }
}