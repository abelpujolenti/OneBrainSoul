using AI.Combat.AbilitySpecs;
using UnityEngine;

namespace Interfaces.AI.Combat
{
    public interface IProjectileAbility : ITarget
    {
        public void Activate();

        public void GoesOnAutomatic(bool goesOnAutomatic, Vector3 direction);

        public AbilityCast GetCast();

        public bool FIREEEEEEEEEEEEEE();
    }
}