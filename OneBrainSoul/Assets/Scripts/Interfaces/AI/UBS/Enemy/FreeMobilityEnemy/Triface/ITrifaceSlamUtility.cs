using Interfaces.AI.UBS.Enemy.FreeMobilityEnemy.Triface.BaseInterfaces;

namespace Interfaces.AI.UBS.Enemy.FreeMobilityEnemy.Triface
{
    public interface ITrifaceSlamUtility : ITrifaceHasATargetForSlam
    {
        public bool IsSlamOnCooldown();

        public bool IsSlamTargetInsideDetectionArea();
    }
}