using Interfaces.AI.UBS.BaseInterfaces.Get;

namespace Interfaces.AI.UBS.Ally
{
    public interface IAllyGetCloserToRivalUtility : IHasATarget, IIsAttacking, IGetDistanceToRival, IGetRemainingDistance, 
        IGetMinimumRangeToAttack, IGetMaximumRangeToAttack, IIsUnderAttack
    {}
}