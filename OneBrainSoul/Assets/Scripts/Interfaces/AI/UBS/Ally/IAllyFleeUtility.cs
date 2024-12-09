using Interfaces.AI.UBS.BaseInterfaces.Get;

namespace Interfaces.AI.UBS.Ally
{
    public interface IAllyFleeUtility : IGetDistancesToEnemiesThatTargetsMe, IGetMinimumEnemiesAroundToFlee, IGetAlertRadius, 
        IGetSafetyRadius, IIsFleeing, IIsInFleeState, IGetHealth, IIsUnderThreat
    {}
}