using AI.Combat.ScriptableObjects;

namespace ECS.Components.AI.Combat.Abilities
{
    public class CustomMeshAbilityComponent : AbilityComponent
    {
        private float _scale;

        public CustomMeshAbilityComponent(AgentAbility agentAbility) : base(agentAbility)
        {
            _scale = agentAbility.abilityAoE.scale;
        }

        public float GetScale()
        {
            return _scale;
        }
    }
}