using System;
using AI.Combat.Contexts;
using AI.Combat.Contexts.Target;
using Interfaces.AI.UBS.BaseInterfaces.Get;
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
                new AICombatAgentAction<TrifaceAction>(TrifaceAction.ACQUIRE_NEW_TARGET_FOR_SLAM),
                new AICombatAgentAction<TrifaceAction>(TrifaceAction.GET_CLOSER_TO_TARGET_OF_SLAM),
                new AICombatAgentAction<TrifaceAction>(TrifaceAction.ROTATE),
                new AICombatAgentAction<TrifaceAction>(TrifaceAction.SLAM)
            };

            actions[0].utilityScore = CalculatePatrolUtility(context);
            actions[1].utilityScore = CalculateAcquireNewTargetForSlam(context);
            actions[2].utilityScore = CalculateGetCloserToTargetOfSlamUtility(context);
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

        private static float CalculatePatrolUtility(ITrifaceIdleUtility trifaceIdleUtility)
        {
            return Convert.ToInt16(!trifaceIdleUtility.IsSeeingATarget());
        }

        private static float CalculateAcquireNewTargetForSlam(
            ITrifaceAcquireNewTargetForSlamForSlamForSlamUtility trifaceAcquireNewTargetForSlamForSlamForSlamUtility)
        {
            return Convert.ToInt16(trifaceAcquireNewTargetForSlamForSlamForSlamUtility.IsSeeingATargetForSlam() && 
                                   !trifaceAcquireNewTargetForSlamForSlamForSlamUtility.HasATargetForSlam());
        }

        private static float CalculateGetCloserToTargetOfSlamUtility(
            ITrifaceGetCloserToTargetOfSlamUtility trifaceGetCloserToTargetOfOfSlamUtility)
        {
            return Convert.ToInt16(trifaceGetCloserToTargetOfOfSlamUtility.HasATargetForSlam()) * 0.8f;
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