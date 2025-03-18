using AI.Combat.Contexts;
using Interfaces.AI.UBS.BaseInterfaces.Get;

namespace AI.Combat.Enemy.Sendatu
{
    public class SendatuUtilityFunction : IGetBestAction<SendatuAction, SendatuContext>
    {
        public SendatuAction GetBestAction(SendatuContext context)
        {
            //TODO SENDATU UTILITY FUNCTION
            AICombatAgentAction<SendatuAction>[] actions = new AICombatAgentAction<SendatuAction>[1];

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
    }
}