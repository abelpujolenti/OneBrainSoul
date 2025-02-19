using Interfaces.AI.UBS.Enemy.FreeMobilityEnemy.Triface.BaseInterfaces;

namespace Interfaces.AI.UBS.Enemy.FreeMobilityEnemy.Triface
{
    public interface ITrifaceLoseTargetUtility : ITrifaceHasATargetForSlam
    {
        public bool CanSeeTargetOfSlam();
    }
}