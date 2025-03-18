using Interfaces.AI.UBS.Enemy.TeleportMobilityEnemy.LongArms.BaseInterfaces;

namespace Interfaces.AI.UBS.Enemy.TeleportMobilityEnemy.LongArms
{
    public interface ILongArmsClapAboveUtility : ILongArmsHasATargetForClapAbove
    {
        public bool IsClapAboveOnCooldown();

        public bool IsClapAboveTargetInsideDetectionArea();
    }
}