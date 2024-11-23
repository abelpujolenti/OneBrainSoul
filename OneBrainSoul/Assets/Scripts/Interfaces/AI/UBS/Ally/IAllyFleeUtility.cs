using Interfaces.AI.UBS.BaseInterfaces.Get;

namespace Interfaces.AI.UBS.Ally
{
    public interface IAllyFleeUtility : IGetDistancesToEnemiesThatThreatMe, IGetAlertRadius, IIsFleeing, IIsInFleeState, 
        IGetHealth, IIsUnderThreat
    {}
}