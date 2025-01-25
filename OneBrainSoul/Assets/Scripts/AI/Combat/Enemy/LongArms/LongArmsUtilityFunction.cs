using System;
using AI.Combat.Contexts;
using Interfaces.AI.UBS.BaseInterfaces.Get;
using Interfaces.AI.UBS.Enemy;

namespace AI.Combat.Enemy.LongArms
{
    public class LongArmsUtilityFunction : IGetBestAction<LongArmsAction, LongArmsContext>
    {
        public LongArmsAction GetBestAction(LongArmsContext context)
        {
            AICombatAgentAction<LongArmsAction>[] actions = new[]
            {
                new AICombatAgentAction<LongArmsAction>(LongArmsAction.LOOKING),
                new AICombatAgentAction<LongArmsAction>(LongArmsAction.THROW_ROCK),
                new AICombatAgentAction<LongArmsAction>(LongArmsAction.CLAP_ABOVE),
                new AICombatAgentAction<LongArmsAction>(LongArmsAction.FLEE)
            };

            //TODO LONG ARMS UTILITY METHODS
            /*actions[0].utilityScore = CalculateLookingUtility(context);
            actions[1].utilityScore = CalculateThrowRockUtility(context);
            actions[2].utilityScore = CalculateClapAboveUtility(context);
            actions[5].utilityScore = CalculateFleeUtility(context);*/

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

        private static float CalculateLookingUtility(IEnemyPatrolUtility enemyPatrolUtility)
        {
            //TODO LOOKING
            
            return Convert.ToInt16(!enemyPatrolUtility.IsSeeingPlayer());
        }

        private static float CalculateThrowRockUtility(IEnemyChooseNewRivalUtility enemyChooseNewRivalUtility)
        {
            //TODO LONG ARMS THROW ROCK
            
            return 0;
        }
        
        private static float CalculateClapAboveUtility(IEnemyGetCloserToTargetUtility enemyGetCloserToTargetUtility)
        {
            
            //TODO LONG ARMS CLAP ABOVE
            
            return 1;
        }
        
        private static float CalculateFleeUtility(IEnemyFleeUtility enemyFleeUtility)  
        {
            //TODO LONG ARMS FLEE 
            
            return 0;
        }
    }
}