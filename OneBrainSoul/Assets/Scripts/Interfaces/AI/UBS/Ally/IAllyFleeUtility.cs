using Interfaces.AI.UBS.BaseInterfaces.Get;

namespace Interfaces.AI.UBS.Ally
{
    public interface IAllyFleeUtility : IGetDistancesToEnemiesThatThreatMe, IGetMinimumEnemiesAroundToFlee, IGetAlertRadius, 
        IIsFleeing, IIsInFleeState, IGetHealth, IIsUnderThreat
    {}
}