using AI.Combat.AbilitySpecs;
using AI.Combat.ScriptableObjects;

namespace ECS.Components.AI.Combat.Abilities
{
    public class AreaAbilityComponent
    {
        private readonly bool _doesItTriggerOnTriggerEnter;
        
        private readonly bool _doesItTriggerOnTriggerExit;

        private readonly AbilityAoEType _abilityAoEType;
        private readonly AbilityAoE _abilityAoE;

        private readonly AbilityMovement _abilityMovement;

        public AreaAbilityComponent(AreaAbilityProperties areaAbilityProperties)
        {
            _doesItTriggerOnTriggerEnter = areaAbilityProperties.doesItTriggerOnTriggerEnter;
            
            _doesItTriggerOnTriggerExit = areaAbilityProperties.doesItTriggerOnTriggerExit;

            _abilityAoEType = areaAbilityProperties.abilityAoEType;
            _abilityAoE = areaAbilityProperties.abilityAoE;
            _abilityMovement = areaAbilityProperties.abilityMovement;
        }

        public bool DoesItTriggerOnTriggerEnter()
        {
            return _doesItTriggerOnTriggerEnter;
        }

        public bool DoesItTriggerOnTriggerExit()
        {
            return _doesItTriggerOnTriggerExit;
        }

        public AbilityAoEType GetAoEType()
        {
            return _abilityAoEType;
        }

        public AbilityAoE GetAoE()
        {
            return _abilityAoE;
        }

        public AbilityMovement GetMovement()
        {
            return _abilityMovement;
        }
    }
}