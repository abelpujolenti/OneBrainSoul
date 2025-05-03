using AI.Combat.AbilityProjectiles;
using AI.Combat.AbilitySpecs;
using UnityEngine;

namespace Interfaces.AI.Combat
{
    public interface IProjectileAbility : ITarget
    {
        public Projectile Activate();

        public void Cancel();

        public AbilityCast GetCast();

        public void FIREEEEEEEEEEEEEE(Vector3 forceVector);
    }
}