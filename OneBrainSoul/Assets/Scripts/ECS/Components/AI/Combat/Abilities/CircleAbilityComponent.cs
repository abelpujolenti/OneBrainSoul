using AI.Combat.ScriptableObjects;

namespace ECS.Components.AI.Combat.Abilities
{
    public class CircleAbilityComponent : AbilityComponent
    {
        private float _radius;

        public CircleAbilityComponent(AgentAbility agentAbility) : base(agentAbility)
        {
            _radius = agentAbility.abilityAoE.radius;
        }

        public float GetRadius()
        {
            return _radius;
        }
    }
}