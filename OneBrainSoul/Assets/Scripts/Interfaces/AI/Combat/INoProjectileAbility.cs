using System.Collections;
using AI.Combat.AbilityAoEColliders;
using ECS.Components.AI.Combat.Abilities;

namespace Interfaces.AI.Combat
{
    public interface INoProjectileAbility
    {
        public void StartCastingNoProjectileAbility<TAbilityComponent, TAbilityCollider>
            (TAbilityComponent abilityComponent, TAbilityCollider abilityCollider)
                where TAbilityComponent : AbilityComponent
                where TAbilityCollider : AbilityAoECollider<TAbilityComponent>; 
        
        public IEnumerator StartNoProjectileAbilityCastTimeCoroutine<TAbilityComponent, TAbilityCollider>
            (TAbilityComponent abilityComponent, TAbilityCollider abilityCollider) 
                where TAbilityComponent : AbilityComponent
                where TAbilityCollider : AbilityAoECollider<TAbilityComponent>;
    }
}