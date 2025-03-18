using AI.Combat.AbilitySpecs;
using FMODUnity;
using UnityEngine;

namespace AI.Combat.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Area Ability Properties", menuName = "ScriptableObjects/AI/Combat/Abilities/Area Ability Properties", order = 1)]
    public class AreaAbilityProperties : BasicAbilityProperties
    {
        public EventReference abilityAoESound;
        
        public AbilityAoEType abilityAoEType;
        public AbilityAoE abilityAoE = new AbilityAoE();
        public AbilityMovement abilityMovement = new AbilityMovement();

        public bool doesItTriggerOnTriggerEnter;
        public bool doesItTriggerOnTriggerExit;
    }
}