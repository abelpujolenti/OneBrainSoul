using System;
using AI.Combat.Contexts;
using Interfaces.AI.UBS.BaseInterfaces.Get;
using Interfaces.AI.UBS.Enemy.TeleportMobilityEnemy.LongArms;
using UnityEngine;

namespace AI.Combat.Enemy.LongArms
{
    public class LongArmsUtilityFunction : IGetBestAction<LongArmsAction, LongArmsContext>
    {
        public LongArmsAction GetBestAction(LongArmsContext context)
        {
            AICombatAgentAction<LongArmsAction>[] actions = new[]
            {
                new AICombatAgentAction<LongArmsAction>(LongArmsAction.OBSERVE),
                new AICombatAgentAction<LongArmsAction>(LongArmsAction.ACQUIRE_NEW_TARGET_FOR_THROW_ROCK),
                new AICombatAgentAction<LongArmsAction>(LongArmsAction.ACQUIRE_NEW_TARGET_FOR_CLAP_ABOVE),
                new AICombatAgentAction<LongArmsAction>(LongArmsAction.THROW_ROCK),
                new AICombatAgentAction<LongArmsAction>(LongArmsAction.CLAP_ABOVE),
                new AICombatAgentAction<LongArmsAction>(LongArmsAction.FLEE)
            };

            actions[0].utilityScore = CalculateObservingUtility(context);
            actions[1].utilityScore = CalculateAcquireNewTargetForThrowRockUtility(context);
            actions[2].utilityScore = CalculateAcquireNewTargetForClapAboveUtility(context);
            actions[3].utilityScore = CalculateThrowRockUtility(context);
            actions[4].utilityScore = CalculateClapAboveUtility(context);
            actions[5].utilityScore = CalculateFleeUtility(context);

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

        private static float CalculateObservingUtility(ILongArmsIdleUtility longArmsIdleUtility)
        {
            return Convert.ToInt16(!longArmsIdleUtility.IsSeeingATarget()) * 0.6f;
        }

        private static float CalculateAcquireNewTargetForThrowRockUtility(
            ILongArmsAcquireNewTargetForThrowRockUtility longArmsAcquireNewTargetForThrowRockUtility)
        {
            return Convert.ToInt16(longArmsAcquireNewTargetForThrowRockUtility.IsSeeingATargetForThrowRock() &&
                                   !longArmsAcquireNewTargetForThrowRockUtility.HasATargetForThrowRock());
        }

        private static float CalculateAcquireNewTargetForClapAboveUtility(
            ILongArmsAcquireNewTargetForClapAboveUtility longArmsAcquireNewTargetForClapAboveUtility)
        {
            return Convert.ToInt16(longArmsAcquireNewTargetForClapAboveUtility.IsSeeingATargetForClapAbove() &&
                                   !longArmsAcquireNewTargetForClapAboveUtility.HasATargetForClapAbove());
        }

        private static float CalculateThrowRockUtility(ILongArmsThrowRockUtility longArmsThrowRockUtility)
        {
            Debug.Log(longArmsThrowRockUtility.HasATargetForThrowRock());
            
            if (longArmsThrowRockUtility.IsThrowRockOnCooldown() || !longArmsThrowRockUtility.HasATargetForThrowRock())
            {
                return 0;
            }

            return Convert.ToInt16(longArmsThrowRockUtility.IsThrowRockTargetInsideDetectionArea()) * 0.7f;
        }

        private static float CalculateClapAboveUtility(ILongArmsClapAboveUtility longArmsClapAboveUtility)
        {
            if (longArmsClapAboveUtility.IsClapAboveOnCooldown() || !longArmsClapAboveUtility.HasATargetForClapAbove())
            {
                return 0;
            }

            return Convert.ToInt16(longArmsClapAboveUtility.IsClapAboveTargetInsideDetectionArea()) * 0.9f;
        }
        
        private static float CalculateFleeUtility(ILongArmsFleeUtility longArmsFleeUtility)  
        {
            if (longArmsFleeUtility.GetLongArmsBasesFree() == 0)
            {
                return 0;
            }

            return Convert.ToInt16(longArmsFleeUtility.GetDistanceToClosestTargetToFleeFrom() <=
                                   longArmsFleeUtility.GetRadiusToFlee()) * 0.8f;
        }
    }
}