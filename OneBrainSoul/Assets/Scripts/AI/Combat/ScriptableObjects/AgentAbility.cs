using AI.Combat.AbilitySpecs;
using UnityEngine;

namespace AI.Combat.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Combat Agent Ability Properties", menuName = "ScriptableObjects/AI/Combat/Combat Agent Ability Properties", order = 0)]
    public class AgentAbility : ScriptableObject
    {
        public AbilityTarget abilityTarget;

        public AbilityEffectType abilityEffectType;
        public AbilityEffect abilityEffect = new AbilityEffect();

        public AbilityCastType abilityCastType;
        public AbilityCast abilityCast = new AbilityCast();

        public AbilityAoEType abilityAoEType;
        public AbilityAoE abilityAoE = new AbilityAoE();

        public AbilityProjectileType abilityProjectileType;
        public AbilityProjectile abilityProjectile = new AbilityProjectile();
        public GameObject projectilePrefab;
    }
}