using AI.Combat.AbilitySpecs;
using AI.Combat.ScriptableObjects;
using ECS.Entities;
using FMODUnity;

namespace ECS.Components.AI.Combat.Abilities
{
    public class BasicAbilityComponent
    {
        private readonly EntityType _entityAffectedByTheAbility;

        private readonly AbilityCast _abilityCast;

        private readonly AbilityTrigger _abilityTrigger;
        
        private readonly AbilityEffectOnHealthType _abilityEffectOnHealthTypeOnStart;
        private readonly AbilityEffect _abilityEffectOnStart;
        
        private readonly AbilityEffectOnHealthType _abilityEffectOnHealthTypeOnTheDuration;
        private readonly AbilityEffect _abilityEffectOnTheDuration;
        
        private readonly AbilityEffectOnHealthType _abilityEffectOnHealthTypeOnEnd;
        private readonly AbilityEffect _abilityEffectOnEnd;

        public BasicAbilityComponent(BasicAbilityProperties basicAbilityProperties)
        {
            _entityAffectedByTheAbility = basicAbilityProperties.typesAffectedByTheAbility;

            _abilityCast = basicAbilityProperties.abilityCast.DeepCopy();

            _abilityTrigger = basicAbilityProperties.abilityTrigger;
            
            _abilityEffectOnHealthTypeOnStart = basicAbilityProperties.abilityEffectOnHealthTypeOnStart;
            _abilityEffectOnStart = basicAbilityProperties.abilityEffectOnStart;
            
            _abilityEffectOnHealthTypeOnTheDuration = basicAbilityProperties.abilityEffectOnHealthTypeOnTheDuration;
            _abilityEffectOnTheDuration = basicAbilityProperties.abilityEffectOnTheDuration;
            
            _abilityEffectOnHealthTypeOnEnd = basicAbilityProperties.abilityEffectOnHealthTypeOnEnd;
            _abilityEffectOnEnd = basicAbilityProperties.abilityEffectOnEnd;
        }

        public EntityType GetAffectedEntities()
        {
            return _entityAffectedByTheAbility;
        }

        public AbilityCast GetCast()
        {
            return _abilityCast;
        }

        public AbilityTrigger GetTrigger()
        {
            return _abilityTrigger;
        }

        public AbilityEffectOnHealthType GetEffectTypeOnStart()
        {
            return _abilityEffectOnHealthTypeOnStart;
        }

        public AbilityEffect GetEffectOnStart()
        {
            return _abilityEffectOnStart;
        }

        public AbilityEffectOnHealthType GetEffectTypeOnTheDuration()
        {
            return _abilityEffectOnHealthTypeOnTheDuration;
        }

        public AbilityEffect GetEffectOnTheDuration()
        {
            return _abilityEffectOnTheDuration;
        }

        public AbilityEffectOnHealthType GetEffectTypeOnEnd()
        {
            return _abilityEffectOnHealthTypeOnEnd;
        }

        public AbilityEffect GetEffectOnEnd()
        {
            return _abilityEffectOnEnd;
        }
    }
}