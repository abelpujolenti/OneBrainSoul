using System;
using AI.Combat.Contexts;
using AI.Combat.ScriptableObjects;
using UnityEngine;

namespace ECS.Entities.AI.Combat
{
    public abstract class TeleportMobilityEnemy<TContext, TAction> : AIEnemy<TeleportMobilityEnemyProperties, TContext, TAction>
        where TContext : TeleportMobilityEnemyContext
        where TAction : Enum
    {
        protected override void EnemySetup(float radius, TeleportMobilityEnemyProperties teleportMobilityEnemyProperties, EntityType entityType)
        {
            base.EnemySetup(radius, teleportMobilityEnemyProperties, entityType);
        }

        protected override void GoToArea(Vector3 estimatedPosition)
        {
            
        }

        protected override void OnEndInvestigation()
        {
            base.OnEndInvestigation();
            //TODO TELEPORT MOBILITY ENEMY CHOOSE NEW DESTINATION
        }
    }
}