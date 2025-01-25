using System;
using AI.Combat.Contexts;
using Interfaces.AI.UBS.BaseInterfaces.Get;
using Interfaces.AI.UBS.Enemy;
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

            //TODO TRIFACE UTILITY METHODS
            /*actions[0].utilityScore = CalculatePatrolUtility(context);
            actions[1].utilityScore = CalculateLookForRivalUtility(context);
            actions[2].utilityScore = CalculateGetCloserToRivalUtility(context);
            actions[3].utilityScore = 0.1f;
            actions[4].utilityScore = CalculateSlamUtility(context);*/

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
            return Convert.ToInt16(!enemyPatrolUtility.IsSeeingPlayer());
        }

        private static float CalculateLookForRivalUtility(IEnemyChooseNewRivalUtility enemyChooseNewRivalUtility)
        {
            if (!enemyChooseNewRivalUtility.IsSeeingPlayer())
            {
                return 0;
            }
            
            if (!enemyChooseNewRivalUtility.HasATarget())
            {
                return 1f;
            }
            
            return 0;
        }
        
        private static float CalculateGetCloserToRivalUtility(IEnemyGetCloserToTargetUtility enemyGetCloserToTargetUtility)
        {
            if (!enemyGetCloserToTargetUtility.HasATarget() || enemyGetCloserToTargetUtility.IsAttacking())
            {
                return 0;
            }
            
            float distanceToRival = enemyGetCloserToTargetUtility.GetDistanceToTarget();
            
            return 1;
        }

        private static float CalculateSlamUtility(IEnemyAttackUtility enemyAttackUtility)
        {
            if (!enemyAttackUtility.HasATarget())
            {
                return 0;
            }

            float distanceToRival = enemyAttackUtility.GetDistanceToTarget();

            if (Vector3.Angle(enemyAttackUtility.GetAgentTransform().forward, enemyAttackUtility.GetVectorToTarget()) < 15f)
            {
                return 0.9f;
            }
            
            return 0;
        }
    }
}