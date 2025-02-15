using System;
using AI.Combat.Contexts;
using AI.Combat.Contexts.Target;
using Interfaces.AI.UBS.BaseInterfaces.Get;
using Interfaces.AI.UBS.Enemy.LongArms;
using Vector3 = UnityEngine.Vector3;

namespace AI.Combat.Enemy.LongArms
{
    public class LongArmsUtilityFunction : IGetBestAction<LongArmsAction, LongArmsContext>
    {
        public LongArmsAction GetBestAction(LongArmsContext context)
        {
            AICombatAgentAction<LongArmsAction>[] actions = new[]
            {
                new AICombatAgentAction<LongArmsAction>(LongArmsAction.OBSERVING),
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
            if (longArmsThrowRockUtility.IsThrowRockOnCooldown() || !longArmsThrowRockUtility.HasATargetForThrowRock())
            {
                return 0;
            }

            TargetContext throwRockTargetContext = longArmsThrowRockUtility.GetThrowRockTargetContext();

            float distanceToTarget = throwRockTargetContext.GetDistanceToTarget();

            return Convert.ToInt16(
                longArmsThrowRockUtility.GetThrowRockMinimRangeToCast() < distanceToTarget &&
                longArmsThrowRockUtility.GetThrowRockMaximRangeToCast() > distanceToTarget &&
                Vector3.Angle(longArmsThrowRockUtility.GetDirectionOfThrowRockDetection(), throwRockTargetContext.GetVectorToTarget()) <
                longArmsThrowRockUtility.GetMinimumAngleFromForwardToCastThrowRock()) * 0.8f;
        }
        
        private static float CalculateClapAboveUtility(ILongArmsClapAboveUtility longArmsClapAboveUtility)
        {
            if (longArmsClapAboveUtility.IsClapAboveOnCooldown() || !longArmsClapAboveUtility.HasATargetForClapAbove())
            {
                return 0;
            }

            TargetContext clapAboveTargetContext = longArmsClapAboveUtility.GetClapAboveTargetContext();

            float distanceToTarget = clapAboveTargetContext.GetDistanceToTarget();

            return Convert.ToInt16(
                longArmsClapAboveUtility.GetClapAboveMinimRangeToCast() < distanceToTarget &&
                longArmsClapAboveUtility.GetClapAboveMaximRangeToCast() > distanceToTarget &&
                Vector3.Angle(longArmsClapAboveUtility.GetDirectionOfClapAboveDetection(), clapAboveTargetContext.GetVectorToTarget()) <
                longArmsClapAboveUtility.GetMinimumAngleFromForwardToCastClapAbove());
        }
        
        private static float CalculateFleeUtility(ILongArmsFleeUtility longArmsFleeUtility)  
        {
            if (longArmsFleeUtility.GetLongArmsBasesFree() == 0)
            {
                return 0;
            }

            if (longArmsFleeUtility.GetDistanceToClosestTargetToFleeFrom() <= longArmsFleeUtility.GetRadiusToFlee())
            {
                return 0.9f;
            }    
            
            return 0;
        }
    }
}