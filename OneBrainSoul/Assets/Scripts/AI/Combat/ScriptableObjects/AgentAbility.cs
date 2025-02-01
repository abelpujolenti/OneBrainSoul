using AI.Combat.AbilitySpecs;
using UnityEngine;

namespace AI.Combat.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Combat Agent Ability Properties", menuName = "ScriptableObjects/AI/Combat/Combat Agent Ability Properties", order = 0)]
    public class AgentAbility : ScriptableObject
    {
        public AbilityTarget abilityTarget;

        public AbilityCastType abilityCastType;
        public AbilityCast abilityCast = new AbilityCast();
        
        public AbilityProjectile abilityProjectile = new AbilityProjectile();

        public AbilityAoEType abilityAoEType;
        public AbilityAoE abilityAoE = new AbilityAoE();

        public AbilityTrigger abilityTrigger = new AbilityTrigger();

        public AbilityEffectOnHealthType abilityEffectOnHealthTypeOnStart;
        public AbilityEffect abilityEffectOnStart = new AbilityEffect();
        public bool doesItTriggerOnTriggerEnter;

        public AbilityEffectOnHealthType abilityEffectOnHealthTypeOnTheDuration;
        public AbilityEffect abilityEffectOnTheDuration = new AbilityEffect();

        public AbilityEffectOnHealthType abilityEffectOnHealthTypeOnEnd;
        public AbilityEffect abilityEffectOnEnd = new AbilityEffect();
        public bool doesItTriggerOnTriggerExit;
    }
}