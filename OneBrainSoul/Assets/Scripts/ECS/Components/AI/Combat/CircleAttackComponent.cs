using AI.Combat.ScriptableObjects;

namespace ECS.Components.AI.Combat
{
    public class CircleAttackComponent : AttackComponent
    {
        private float _radius;

        public CircleAttackComponent(CombatAgentAbility combatAgentAbility) : base(combatAgentAbility)
        {
            _radius = combatAgentAbility.abilityAoE.GetRadius();
        }

        public float GetRadius()
        {
            return _radius;
        }
    }
}