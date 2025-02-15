using System.Collections.Generic;
using AI.Combat.AbilitySpecs;
using ECS.Components.AI.Combat.Abilities;
using ECS.Entities.AI;

namespace AI.Combat.AbilityCasts
{
    public class BasicAbility
    {
        private BasicAbilityComponent _abilityComponent;

        private List<AgentEntity> _abilityTargets = new List<AgentEntity>();
        
        public BasicAbility(BasicAbilityComponent abilityComponent)
        {
            _abilityComponent = abilityComponent;
        }

        public void Start()
        {
            //TODO BASIC ABILITY START
        }

        public void End()
        {
            //TODO BASIC ABILITY END
        }

        public AbilityCast GetCast()
        {
            return _abilityComponent.GetCast();
        }

        public void AddTarget(AgentEntity target)
        {
            _abilityTargets.Add(target);
        }

        public void RemoveTarget(AgentEntity target)
        {
            _abilityTargets.Remove(target);
        }
    }
}