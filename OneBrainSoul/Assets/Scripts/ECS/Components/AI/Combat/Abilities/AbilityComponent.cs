using AI.Combat.AbilitySpecs;
using AI.Combat.ScriptableObjects;

namespace ECS.Components.AI.Combat.Abilities
{
    public abstract class AbilityComponent
    {
        private AbilityTarget _abilityTarget;

        private AbilityCastType _abilityCastType;
        private AbilityCast _abilityCast;

        private AbilityTrigger _abilityTrigger;
        
        private AbilityEffectOnHealthType _abilityEffectOnHealthTypeOnStart;
        private AbilityEffect _abilityEffectOnStart;
        private bool _doesItTriggerOnTriggerEnter;
        
        private AbilityEffectOnHealthType _abilityEffectOnHealthTypeOnTheDuration;
        private AbilityEffect _abilityEffectOnTheDuration;
        
        private AbilityEffectOnHealthType _abilityEffectOnHealthTypeOnEnd;
        private AbilityEffect _abilityEffectOnEnd;
        private bool _doesItTriggerOnTriggerExit;

        private AbilityAoEType _abilityAoEType;
        private AbilityAoE _abilityAoE;

        private AbilityProjectile _abilityProjectile;

        protected AbilityComponent(AgentAbility agentAbility)
        {
            _abilityTarget = agentAbility.abilityTarget;

            _abilityCastType = agentAbility.abilityCastType;
            _abilityCast = agentAbility.abilityCast.DeepCopy();

            _abilityTrigger = agentAbility.abilityTrigger;
            
            _abilityEffectOnHealthTypeOnStart = agentAbility.abilityEffectOnHealthTypeOnStart;
            _abilityEffectOnStart = agentAbility.abilityEffectOnStart;
            _doesItTriggerOnTriggerEnter = agentAbility.doesItTriggerOnTriggerEnter;
            
            _abilityEffectOnHealthTypeOnTheDuration = agentAbility.abilityEffectOnHealthTypeOnTheDuration;
            _abilityEffectOnTheDuration = agentAbility.abilityEffectOnTheDuration;
            
            _abilityEffectOnHealthTypeOnEnd = agentAbility.abilityEffectOnHealthTypeOnEnd;
            _abilityEffectOnEnd = agentAbility.abilityEffectOnEnd;
            _doesItTriggerOnTriggerExit = agentAbility.doesItTriggerOnTriggerExit;

            _abilityAoEType = agentAbility.abilityAoEType;
            _abilityAoE = agentAbility.abilityAoE;

            _abilityProjectile = agentAbility.abilityProjectile;
        }

        public AbilityTarget GetTarget()
        {
            return _abilityTarget;
        }

        public AbilityCastType GetCastType()
        {
            return _abilityCastType;
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

        public bool DoesItTriggerOnTriggerEnter()
        {
            return _doesItTriggerOnTriggerEnter;
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

        public AbilityProjectile GetProjectile()
        {
            return _abilityProjectile;
        }
    }
}