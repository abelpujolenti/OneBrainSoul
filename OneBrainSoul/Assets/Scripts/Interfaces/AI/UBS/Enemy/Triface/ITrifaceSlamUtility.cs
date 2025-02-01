using Interfaces.AI.UBS.BaseInterfaces.Get;

namespace Interfaces.AI.UBS.Enemy.Triface
{
    public interface ITrifaceSlamUtility : IHasATarget, IGetAgentTransform, IGetTargetTransform,
        IGetVectorToTarget, IGetDistanceToTarget
    {
        public float GetMinimumRangeToCast();
        
        public float GetMaximumRangeToCast();
        
        public bool IsOnCooldown();
    }
}