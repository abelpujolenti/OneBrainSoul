using System;
using System.Collections.Generic;
using AI.Combat.ScriptableObjects;
using Interfaces.AI.UBS.Ally;
using Interfaces.AI.UBS.BaseInterfaces.Get;
using UnityEngine;

namespace AI.Combat.Ally
{
    public class AIAllyUtilityFunction : IGetBestAction<AIAllyAction, AIAllyContext>
    {
        public AIAllyAction GetBestAction(AIAllyContext context)
        {
            AICombatAgentAction<AIAllyAction>[] actions = new[]
            {
                new AICombatAgentAction<AIAllyAction>(AIAllyAction.FOLLOW_PLAYER),
                new AICombatAgentAction<AIAllyAction>(AIAllyAction.CHOOSE_NEW_RIVAL),
                new AICombatAgentAction<AIAllyAction>(AIAllyAction.GET_CLOSER_TO_RIVAL),
                new AICombatAgentAction<AIAllyAction>(AIAllyAction.ROTATE),
                new AICombatAgentAction<AIAllyAction>(AIAllyAction.ATTACK),
                new AICombatAgentAction<AIAllyAction>(AIAllyAction.FLEE),
                new AICombatAgentAction<AIAllyAction>(AIAllyAction.DODGE_ATTACK)
            };

            actions[0].utilityScore = CalculateFollowPlayerUtility(context);
            actions[1].utilityScore = CalculateChooseNewRivalUtility(context);
            actions[2].utilityScore = CalculateGetCloserToRivalUtility(context);
            actions[3].utilityScore = 0.1f;
            actions[4].utilityScore = CalculateAttackUtility(context);
            actions[5].utilityScore = CalculateFleeUtility(context);
            actions[6].utilityScore = CalculateDodgeAttackUtility(context);

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

        private static float CalculateFollowPlayerUtility(IAllyFollowPlayerUtility allyFollowPlayerUtility)
        {
            return Convert.ToUInt16(allyFollowPlayerUtility.IsInRetreatState() || !allyFollowPlayerUtility.IsSeeingARival());
        }
        
        private static float CalculateChooseNewRivalUtility(IAllyChooseNewRivalUtility allyChooseNewRivalUtility)
        {
            if (!allyChooseNewRivalUtility.HasATarget())
            {
                return 0.9f;
            }
            
            return 0;
        }

        private static float CalculateGetCloserToRivalUtility(IAllyGetCloserToRivalUtility allyGetCloserToRivalUtility)
        {
            if (!allyGetCloserToRivalUtility.HasATarget() || allyGetCloserToRivalUtility.IsAttacking())
            {
                return 0;
            }

            if (allyGetCloserToRivalUtility.GetMaximumRangeToAttack() > allyGetCloserToRivalUtility.GetDistanceToRival())
            {
                return 0;
            }
            
            return 0.7f;
        }

        private static float CalculateAttackUtility(IAllyAttackUtility allyAttackUtility)
        {
            if (!allyAttackUtility.HasATarget())
            {
                return 0;
            }
            
            float distanceToRival = allyAttackUtility.GetDistanceToRival();

            if (allyAttackUtility.GetMinimumRangeToAttack() > distanceToRival ||
                allyAttackUtility.GetMaximumRangeToAttack() < distanceToRival ||
                Vector3.Angle(allyAttackUtility.GetAgentTransform().forward, allyAttackUtility.GetVectorToRival()) >= 15f)
            {
                return 0;
            }
            
            if (allyAttackUtility.IsInAttackState())
            {
                return 1;
            }

            if (allyAttackUtility.CanDefeatEnemy())
            {
                return 0.9f;
            }

            if (allyAttackUtility.CanStunEnemy())
            {
                return 0.6f;
            }
            
            return 0.4f;
        }
        
        private static float CalculateFleeUtility(IAllyFleeUtility allyFleeUtility)  
        {
            if (allyFleeUtility.IsInFleeState())
            {
                return 1;
            }

            List<float> distancesToEnemies = allyFleeUtility.GetDistancesToEnemiesThatTargetsMe();

            float radius = allyFleeUtility.IsFleeing() ? allyFleeUtility.GetAlertRadius() : allyFleeUtility.GetSafetyRadius();

            uint minimumEnemiesAroundToFlee = allyFleeUtility.GetMinimumEnemiesAroundToFlee();
            uint enemiesCounter = 0;

            foreach (float distance in distancesToEnemies)
            {
                if (distance > radius)
                {
                    continue;
                }

                enemiesCounter++;
                
                if (enemiesCounter < minimumEnemiesAroundToFlee)
                {
                    continue;
                }

                return 0.8f;
            }
            
            return 0;
        }
        
        private static float CalculateDodgeAttackUtility(IAllyDodgeAttackUtility allyDodgeAttackUtility)
        {
            if (!allyDodgeAttackUtility.IsUnderAttack())
            {
                return 0;
            }

            /*if (allyDodgeAttackUtility.GetHealth() < allyDodgeAttackUtility.GetTotalHealth() * 0.3f)
            {
                return 0.8f;
            }*/

            //return 0.8f;
            return 0f;
        }
    }
}