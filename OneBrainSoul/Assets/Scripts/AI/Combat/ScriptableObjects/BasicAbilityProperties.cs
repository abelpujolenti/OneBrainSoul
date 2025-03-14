using AI.Combat.AbilitySpecs;
using ECS.Entities;
using UnityEngine;

namespace AI.Combat.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Direct Ability Properties", menuName = "ScriptableObjects/AI/Combat/Abilities/Direct Ability Properties", order = 0)]
    public class BasicAbilityProperties : ScriptableObject
    {
        public bool canAffectCaster;
        public EntityType abilityTarget;
        public EntityType typesAffectedByTheAbility;

        public AbilityCast abilityCast = new AbilityCast();

        public AbilityTrigger abilityTrigger = new AbilityTrigger();
        
        public AbilityEffectOnHealthType abilityEffectOnHealthTypeOnStart;
        public AbilityEffect abilityEffectOnStart = new AbilityEffect();

        public AbilityEffectOnHealthType abilityEffectOnHealthTypeOnTheDuration;
        public AbilityEffect abilityEffectOnTheDuration = new AbilityEffect();

        public AbilityEffectOnHealthType abilityEffectOnHealthTypeOnEnd;
        public AbilityEffect abilityEffectOnEnd = new AbilityEffect();
    }
}