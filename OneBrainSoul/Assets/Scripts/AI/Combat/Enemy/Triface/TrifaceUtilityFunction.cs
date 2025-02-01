using System;
using AI.Combat.Contexts;
using Interfaces.AI.UBS.BaseInterfaces.Get;
using Interfaces.AI.UBS.Enemy;
using Interfaces.AI.UBS.Enemy.Triface;
using UnityEngine;

namespace AI.Combat.Enemy.Triface
{
    public class TrifaceUtilityFunction : IGetBestAction<TrifaceAction, TrifaceContext>
    {
        public TrifaceAction GetBestAction(TrifaceContext context)
        {
            AICombatAgentAction<TrifaceAction>[] actions = new[]
            {
                new AICombatAgentAction<TrifaceAction>(TrifaceAction.PATROL),
                new AICombatAgentAction<TrifaceAction>(TrifaceAction.LOOK_FOR_PLAYER),
                new AICombatAgentAction<TrifaceAction>(TrifaceAction.GET_CLOSER_TO_PLAYER),
                new AICombatAgentAction<TrifaceAction>(TrifaceAction.ROTATE),
                new AICombatAgentAction<TrifaceAction>(TrifaceAction.SLAM)
            };

            actions[0].utilityScore = CalculatePatrolUtility(context);
            actions[1].utilityScore = CalculateLookForPlayer(context);
            actions[2].utilityScore = CalculateGetCloserToTargetUtility(context);
            actions[3].utilityScore = 0.1f;
            actions[4].utilityScore = CalculateSlamUtility(context);

            uint index = 0;

            for (uint i = 1; i < actions.Length; i++)
            {
                if (actions[i].utilityScore < actions[index].utilityScore)
                {
                    continue;
                }

                index = i;
            }
            
            return actions[index].GetAIAction();
        }

        private static float CalculatePatrolUtility(IEnemyPatrolUtility enemyPatrolUtility)
        {
            return Convert.ToInt16(!enemyPatrolUtility.IsSeeingATarget());
        }

        private static float CalculateLookForPlayer(IEnemyLookForNewTargetUtility enemyLookForNewTargetUtility)
        {
            return Convert.ToInt16(enemyLookForNewTargetUtility.IsSeeingATarget() && !enemyLookForNewTargetUtility.HasATarget());
        }

        private static float CalculateGetCloserToTargetUtility(IEnemyGetCloserToTargetUtility enemyGetCloserToTargetUtility)
        {
            return Convert.ToInt16(enemyGetCloserToTargetUtility.HasATarget()) * 0.8f;
        }

        private static float CalculateSlamUtility(ITrifaceSlamUtility trifaceSlamUtility)
        {
            if (!trifaceSlamUtility.HasATarget() || trifaceSlamUtility.IsOnCooldown())
            {
                return 0;
            }

            float distanceToRival = trifaceSlamUtility.GetDistanceToTarget();

            return Convert.ToInt16(
                trifaceSlamUtility.GetMinimumRangeToCast() < distanceToRival &&
                trifaceSlamUtility.GetMaximumRangeToCast() > distanceToRival &&
                Vector3.Angle(trifaceSlamUtility.GetAgentTransform().forward, trifaceSlamUtility.GetVectorToTarget()) < 15f);
        }
    }
}