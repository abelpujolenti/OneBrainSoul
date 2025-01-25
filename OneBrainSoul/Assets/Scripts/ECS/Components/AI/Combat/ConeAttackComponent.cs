using AI.Combat.AbilitySpecs;
using AI.Combat.ScriptableObjects;
using UnityEngine;

namespace ECS.Components.AI.Combat
{
    public class ConeAttackComponent : AttackComponent
    {
        private Vector3 _direction;
        private float _radius;
        private float _degrees;

        public ConeAttackComponent(CombatAgentAbility combatAgentAbility) : base(combatAgentAbility)
        {
            AbilityAoE abilityAoE = combatAgentAbility.abilityAoE;
            
            _direction = abilityAoE.GetDirection();
            _radius = abilityAoE.GetRadius();
            _degrees = abilityAoE.GetDegrees();
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