using AI.Combat.AbilitySpecs;

namespace Interfaces.AI.Combat
{
    public interface IAreaAbility : ITarget
    {
        public void Activate();

        public AbilityCast GetCast();
    }
}