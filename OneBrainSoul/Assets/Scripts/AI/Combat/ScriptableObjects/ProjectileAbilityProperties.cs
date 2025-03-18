using AI.Combat.AbilitySpecs;
using FMODUnity;
using UnityEngine;

namespace AI.Combat.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Projectile Ability Properties", menuName = "ScriptableObjects/AI/Combat/Abilities/Projectile Ability Properties", order = 2)]
    public class ProjectileAbilityProperties : AreaAbilityProperties
    {
        public EventReference projectileSound;
        
        public AbilityProjectile abilityProjectile = new AbilityProjectile();
    }
}