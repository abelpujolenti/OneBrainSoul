using Interfaces.AI.UBS.Enemy.TeleportMobilityEnemy.LongArms.BaseInterfaces;

namespace Interfaces.AI.UBS.Enemy.TeleportMobilityEnemy.LongArms
{
    public interface ILongArmsThrowRockUtility : ILongArmsHasATargetForThrowRock
    {
        public bool IsThrowRockOnCooldown();

        public bool IsThrowRockTargetInsideDetectionArea();
    }
}