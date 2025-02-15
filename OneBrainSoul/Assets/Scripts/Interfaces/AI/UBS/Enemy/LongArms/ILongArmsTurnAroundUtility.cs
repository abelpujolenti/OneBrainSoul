using Interfaces.AI.UBS.BaseInterfaces.Get;

namespace Interfaces.AI.UBS.Enemy.LongArms
{
    public interface ILongArmsTurnAroundUtility : IIsSeeingATarget
    {
        public bool IsTimeToTurnAround();
    }
}