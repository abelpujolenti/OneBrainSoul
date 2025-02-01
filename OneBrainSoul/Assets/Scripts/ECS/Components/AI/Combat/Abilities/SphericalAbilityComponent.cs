using AI.Combat.ScriptableObjects;

namespace ECS.Components.AI.Combat.Abilities
{
    public class SphericalAbilityComponent : AbilityComponent
    {
        private float _radius;

        public SphericalAbilityComponent(AgentAbility agentAbility) : base(agentAbility)
        {
            _radius = agentAbility.abilityAoE.radius;
        }

        public float GetRadius()
        {
            return _radius;
        }
    }
}