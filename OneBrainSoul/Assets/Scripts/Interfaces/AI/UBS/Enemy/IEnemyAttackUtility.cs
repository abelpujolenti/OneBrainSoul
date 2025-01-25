using Interfaces.AI.UBS.BaseInterfaces.Get;

namespace Interfaces.AI.UBS.Enemy
{
    public interface IEnemyAttackUtility : IHasATarget, IGetAgentTransform, IGetTargetTransform, IGetVectorToTarget, 
        IGetDistanceToTarget
    {}
}