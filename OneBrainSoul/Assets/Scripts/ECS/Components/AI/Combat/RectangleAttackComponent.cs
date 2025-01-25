using AI.Combat.AbilitySpecs;
using AI.Combat.ScriptableObjects;
using UnityEngine;

namespace ECS.Components.AI.Combat
{
    public class RectangleAttackComponent : AttackComponent
    {
        private Vector3 _direction;
        private float _length;
        private float _wideness;

        public RectangleAttackComponent(CombatAgentAbility combatAgentAbility) : base(combatAgentAbility)
        {
            AbilityAoE abilityAoE = combatAgentAbility.abilityAoE;
            
            _direction = abilityAoE.GetDirection();
            _length = abilityAoE.GetLength();
            _wideness = abilityAoE.GetWideness();
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