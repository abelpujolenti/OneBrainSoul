using System.Collections.Generic;

namespace Interfaces.AI.UBS.BaseInterfaces.Get
{
    public interface IGetGroupsPriorityHelp
    {
        public Dictionary<uint, float> GetGroupsHelpPriority();
    }
}