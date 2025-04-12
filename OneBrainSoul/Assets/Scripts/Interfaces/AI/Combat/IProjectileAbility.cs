using AI.Combat.AbilitySpecs;
using UnityEngine;

namespace Interfaces.AI.Combat
{
    public interface IProjectileAbility : ITarget
    {
        public void Activate();

        public void Cancel();

        public AbilityCast GetCast();

        public bool FIREEEEEEEEEEEEEE();
    }
}