using System;
using AI.Combat.Contexts;

namespace ECS.Entities.AI.Combat
{
    public abstract class TeleportMobilityEnemy<TContext, TAction> : AIEnemy<TContext, TAction>
        where TContext : AIEnemyContext
        where TAction : Enum
    {
    }
}