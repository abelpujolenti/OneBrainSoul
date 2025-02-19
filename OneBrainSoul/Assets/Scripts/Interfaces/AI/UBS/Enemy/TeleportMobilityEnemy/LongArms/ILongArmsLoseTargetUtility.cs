using Interfaces.AI.UBS.Enemy.TeleportMobilityEnemy.LongArms.BaseInterfaces;

namespace Interfaces.AI.UBS.Enemy.TeleportMobilityEnemy.LongArms
{
    public interface ILongArmsLoseTargetUtility : ILongArmsHasATargetForThrowRock, ILongArmsHasATargetForClapAbove
    {
        public bool CanSeeTargetOfThrowRock();
        
        public bool CanSeeTargetOfClapAbove();
    }
}