using AI.Combat.AbilitySpecs;
using AI.Combat.ScriptableObjects;
using UnityEngine;

namespace ECS.Components.AI.Combat.Abilities
{
    public abstract class AbilityComponent
    {
        private AbilityTarget _abilityTarget;
        
        private AbilityEffectType _abilityEffectType;
        private AbilityEffect _abilityEffect;

        private AbilityCastType _abilityCastType;
        private AbilityCast _abilityCast;

        private AbilityAoEType _abilityAoEType;
        private float _height;

        private AbilityProjectileType _abilityProjectileType;
        private AbilityProjectile _abilityProjectile;
        private GameObject _projectilePrefab;

        protected AbilityComponent(AgentAbility agentAbility)
        {
            _abilityTarget = agentAbility.abilityTarget;
            
            _abilityEffectType = agentAbility.abilityEffectType;
            _abilityEffect = agentAbility.abilityEffect;

            _abilityCastType = agentAbility.abilityCastType;
            _abilityCast = agentAbility.abilityCast.DeepCopy();

            _abilityAoEType = agentAbility.abilityAoEType;
            _height = agentAbility.abilityAoE.height;

            _abilityProjectileType = agentAbility.abilityProjectileType;
            _abilityProjectile = agentAbility.abilityProjectile;
            _projectilePrefab = agentAbility.projectilePrefab;
        }

        public AbilityTarget GetTarget()
        {
            return _abilityTarget;
        }

        public AbilityEffectType GetEffectType()
        {
            return _abilityEffectType;
        }

        public AbilityEffect GetEffect()
        {
            return _abilityEffect;
        }

        public AbilityCastType GetCastType()
        {
            return _abilityCastType;
        }

        public AbilityCast GetCast()
        {
            return _abilityCast;
        }

        public AbilityAoEType GetAoEType()
        {
            return _abilityAoEType;
        }

        public float GetHeight()
        {
            return _height;
        }

        public AbilityProjectileType GetProjectileType()
        {
            return _abilityProjectileType;
        }

        public AbilityProjectile GetProjectile()
        {
            return _abilityProjectile;
        }

        public GameObject GetProjectilePrefab()
        {
            return _projectilePrefab;
        }
    }
}