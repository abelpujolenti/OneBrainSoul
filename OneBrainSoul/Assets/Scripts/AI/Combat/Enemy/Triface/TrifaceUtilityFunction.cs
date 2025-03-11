using System;
using AI.Combat.Contexts;
using AI.Combat.Contexts.Target;
using Interfaces.AI.UBS.BaseInterfaces.Get;
using Interfaces.AI.UBS.Enemy;
using Interfaces.AI.UBS.Enemy.FreeMobilityEnemy;
using Interfaces.AI.UBS.Enemy.FreeMobilityEnemy.Triface;
using UnityEngine;

namespace AI.Combat.Enemy.Triface
{
    public class TrifaceUtilityFunction : IGetBestAction<TrifaceAction, TrifaceContext>
    {
        public TrifaceAction GetBestAction(TrifaceContext context)
        {
            AICombatAgentAction<TrifaceAction>[] actions = new[]
            {
                new AICombatAgentAction<TrifaceAction>(TrifaceAction.CONTINUE_NAVIGATION),
                new AICombatAgentAction<TrifaceAction>(TrifaceAction.ROTATE),
                new AICombatAgentAction<TrifaceAction>(TrifaceAction.PATROL),
                new AICombatAgentAction<TrifaceAction>(TrifaceAction.INVESTIGATE_AREA),
                new AICombatAgentAction<TrifaceAction>(TrifaceAction.GO_TO_CLOSEST_SIGHTED_TARGET),
                new AICombatAgentAction<TrifaceAction>(TrifaceAction.ACQUIRE_NEW_TARGET_FOR_SLAM),
                new AICombatAgentAction<TrifaceAction>(TrifaceAction.SLAM)
            };

            actions[0].utilityScore = 0.1f;
            actions[1].utilityScore = CalculateRotateUtility(context);
            actions[2].utilityScore = CalculatePatrolUtility(context);
            actions[3].utilityScore = CalculateInvestigateAreaUtility(context);
            actions[4].utilityScore = CalculateGoToClosestSightedTarget(context);
            actions[5].utilityScore = CalculateAcquireNewTargetForSlam(context);
            actions[6].utilityScore = CalculateSlamUtility(context);

            uint index = 0;

            for (uint i = 1; i < actions.Length; i++)
            {
                if (actions[i].utilityScore <= actions[index].utilityScore)
                {
                    continue;
                }

                index = i;
            }
            
            return actions[index].GetAIAction();
        }

        private static float CalculateRotateUtility(IFreeMobilityEnemyRotateInSituUtility freeMobilityEnemyRotateInSituUtility)
        {
            return Convert.ToInt16(freeMobilityEnemyRotateInSituUtility.HasStopped()) * 0.75f;
        }

        private static float CalculatePatrolUtility(IFreeMobilityEnemyPatrolUtility freeMobilityEnemyPatrolUtility)
        {
            return Convert.ToInt16(!freeMobilityEnemyPatrolUtility.IsSeeingATarget()) * 0.7f;
        }

        private static float CalculateInvestigateAreaUtility(IFreeMobilityEnemyInvestigateAreaUtility freeMobilityEnemyInvestigateAreaUtility)
        {
            return Convert.ToInt16(!freeMobilityEnemyInvestigateAreaUtility.IsSeeingATarget() &&
                                   freeMobilityEnemyInvestigateAreaUtility.HasReachedDestination()) * 0.8f;
        }

        private static float CalculateGoToClosestSightedTarget(
            IEnemyGoToClosestSightedTarget enemyGoToClosestSightedTargetUtility)
        {
            return Convert.ToInt16(!enemyGoToClosestSightedTargetUtility.HasATarget() && 
                                   enemyGoToClosestSightedTargetUtility.HasAnyTargetBeenSightedInsideCombatArea()) * 0.9f;
        }

        private static float CalculateAcquireNewTargetForSlam(
            ITrifaceAcquireNewTargetForSlamUtility trifaceAcquireNewTargetForSlamUtility)
        {
            return Convert.ToInt16(trifaceAcquireNewTargetForSlamUtility.IsSeeingATargetForSlam() && 
                                   !trifaceAcquireNewTargetForSlamUtility.HasATargetForSlam());
        }

        private static float CalculateSlamUtility(ITrifaceSlamUtility trifaceSlamUtility)
        {
            if (trifaceSlamUtility.IsSlamOnCooldown() || !trifaceSlamUtility.HasATargetForSlam())
            {
                return 0;
            }

            TargetContext slamTargetContext = trifaceSlamUtility.GetSlamTargetContext();

            float distanceToTarget = slamTargetContext.GetDistanceToTarget();

            return Convert.ToInt16(
                trifaceSlamUtility.GetSlamMinimumRangeToCast() < distanceToTarget &&
                trifaceSlamUtility.GetSlamMaximumRangeToCast() > distanceToTarget &&
                Vector3.Angle(trifaceSlamUtility.GetDirectionOfSlamDetection(), slamTargetContext.GetVectorToTarget()) <
                trifaceSlamUtility.GetMinimumAngleFromForwardToCastSlam());
        }
    }
}