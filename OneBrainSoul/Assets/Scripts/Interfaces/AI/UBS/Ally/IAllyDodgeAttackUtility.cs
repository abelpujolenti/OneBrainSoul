﻿using Interfaces.AI.UBS.BaseInterfaces.Get;

namespace Interfaces.AI.UBS.Ally
{
    public interface IAllyDodgeAttackUtility : IIsUnderAttack, IGetHealth, IGetTotalHealth, IGetTimeToNextEnemyMeleeAttack, 
        IGetDistanceToCloserEnemyProjectile, IGetOncomingAttackDamage, IGetRivalHealth
    {}
}