using Interfaces.AI.UBS.BaseInterfaces.Get;

namespace Interfaces.AI.UBS.Enemy
{
    public interface IEnemyGoToClosestSightedTarget : IHasATarget
    {
        public bool HasAnyTargetBeenSightedInsideCombatArea();
    }
}