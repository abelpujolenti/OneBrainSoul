using System.Collections;
using ECS.Components.AI.Combat.Abilities;

namespace Interfaces.AI.Combat
{
    public interface IOmnipresentAbility
    {
        public void StartCastingNoProjectileAbility<TAbilityComponent>(TAbilityComponent abilityComponent)
            where TAbilityComponent : AbilityComponent;
        
        public IEnumerator StartNoProjectileAbilityCastTimeCoroutine<TAbilityComponent>(TAbilityComponent abilityComponent)
            where TAbilityComponent : AbilityComponent;
    }
}