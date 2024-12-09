using Interfaces.AI.Navigation;
using UnityEngine;

namespace Threads
{
    public class GetPositionCommand : BaseCommand
    {
        public IPosition iPosition;

        public ThreadResult<Vector3> result;

        public GetPositionCommand()
        {
            type = CommandReturnType.POSITION;
        }
    }
}