using Interfaces.AI.Navigation;

namespace AI.Navigation
{
    public class DynamicObstacle
    {
        public uint agentId;
        public IPosition iPosition;
        public float radius;
    }
}