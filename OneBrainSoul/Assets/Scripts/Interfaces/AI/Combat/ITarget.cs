namespace Interfaces.AI.Combat
{
    public interface ITarget
    {
        public void SetTargetId(uint targetId);

        public uint GetTargetId();
    }
}