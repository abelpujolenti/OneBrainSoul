using AI.Combat.Contexts;
using Interfaces.AI.UBS.BaseInterfaces.Get;
using Interfaces.AI.UBS.Enemy.Triface;

namespace AI.Combat.Enemy.Sendatu
{
    public class SendatuUtilityFunction : IGetBestAction<SendatuAction, SendatuContext>
    {
        public SendatuAction GetBestAction(SendatuContext context)
        {
            //TODO SENDATU UTILITY FUNCTION
            AICombatAgentAction<SendatuAction>[] actions = new[]
            {
                new AICombatAgentAction<SendatuAction>(SendatuAction.PATROL),
                new AICombatAgentAction<SendatuAction>(SendatuAction.ACQUIRE_NEW_TARGET_FOR_SLAM),
                new AICombatAgentAction<SendatuAction>(SendatuAction.GET_CLOSER_TO_TARGET_OF_SLAM),
                new AICombatAgentAction<SendatuAction>(SendatuAction.ROTATE),
                new AICombatAgentAction<SendatuAction>(SendatuAction.SLAM)
            };

            /*actions[0].utilityScore = CalculatePatrolUtility(context);
            actions[1].utilityScore = CalculateAcquireNewTargetForSlam(context);
            actions[2].utilityScore = CalculateGetCloserToTargetOfSlamUtility(context);
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

        private static float CalculatePatrolUtility(ITrifaceIdleUtility trifaceIdleUtility)
        {
            return 0;
        }

        private static float CalculateAcquireNewTargetForSlam(
            ITrifaceAcquireNewTargetForSlamForSlamForSlamUtility trifaceAcquireNewTargetForSlamForSlamForSlamUtility)
        {
            return 0;
        }

        private static float CalculateGetCloserToTargetOfSlamUtility(
            ITrifaceGetCloserToTargetOfSlamUtility trifaceGetCloserToTargetOfOfSlamUtility)
        {
            return 0;
        }

        private static float CalculateSlamUtility(ITrifaceSlamUtility trifaceSlamUtility)
        {
            return 0;
        }
    }
}