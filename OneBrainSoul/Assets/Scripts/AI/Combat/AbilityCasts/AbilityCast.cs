using ECS.Components.AI.Combat.Abilities;
using UnityEngine;

namespace AI.Combat.AbilityCasts
{
    public abstract class AbilityCast<TAbilityComponent>
        where TAbilityComponent : AbilityComponent
    {
        private TAbilityComponent _abilityComponent;

        private float _timeToCast;

        protected AbilityCast(TAbilityComponent abilityComponent)
        {
            _abilityComponent = abilityComponent;
            _timeToCast = _abilityComponent.GetCast().timeToCast;
        }

        public virtual void StartCast(Transform parent)
        {
            
        }
    }
}