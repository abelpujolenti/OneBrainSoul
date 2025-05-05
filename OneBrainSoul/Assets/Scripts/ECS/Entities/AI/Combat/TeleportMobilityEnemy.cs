using System;
using AI.Combat.Contexts;
using AI.Combat.ScriptableObjects.Enemies;
using UnityEngine;

namespace ECS.Entities.AI.Combat
{
    public abstract class TeleportMobilityEnemy<TContext, TAction> : AIEnemy<TeleportMobilityEnemyProperties, TContext, TAction>
        where TContext : TeleportMobilityEnemyContext
        where TAction : Enum
    {
        protected override void EnemySetup(float radius, TeleportMobilityEnemyProperties teleportMobilityEnemyProperties, 
            EntityType entityType, EntityType targetEntities)
        {
            base.EnemySetup(radius, teleportMobilityEnemyProperties, entityType, targetEntities);
        }

        protected override void OnEndInvestigation()
        {
            base.OnEndInvestigation();
            //TODO TELEPORT MOBILITY ENEMY CHOOSE NEW DESTINATION
        }
    }
}