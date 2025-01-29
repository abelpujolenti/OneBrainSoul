using AI.Combat.AbilitySpecs;
using ECS.Components.AI.Combat.Abilities;

namespace AI.Combat.AbilityCasts
{
    public class AbilityProjectileCast<TAbilityComponent> : AbilityCast<TAbilityComponent>
        where TAbilityComponent : AbilityComponent
    {
        private AbilityProjectileType _abilityProjectileType;
        
        public AbilityProjectileCast(TAbilityComponent abilityComponent) : base(abilityComponent)
        {
            _abilityProjectileType = abilityComponent.GetProjectileType();
        }
    }
}