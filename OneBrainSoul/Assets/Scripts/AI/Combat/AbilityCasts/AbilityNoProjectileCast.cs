using ECS.Components.AI.Combat.Abilities;

namespace AI.Combat.AbilityCasts
{
    public class AbilityNoProjectileCast<TAbilityComponent> : AbilityCast<TAbilityComponent>
        where TAbilityComponent : AbilityComponent
    {
        public AbilityNoProjectileCast(TAbilityComponent abilityComponent) : base(abilityComponent)
        {
            
        }
    }
}