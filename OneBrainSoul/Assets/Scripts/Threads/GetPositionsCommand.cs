using System.Collections.Generic;
using Interfaces.AI.Navigation;
using UnityEngine;

namespace Threads
{
    public class GetPositionsCommand : BaseCommand
    {
        public List<IPosition> iPositions;

        public ThreadResult<List<Vector3>> result;

        public GetPositionsCommand()
        {
            type = CommandReturnType.POSITIONS;
        }
    }
}