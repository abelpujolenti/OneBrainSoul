namespace Interfaces.AI.UBS.Enemy.LongArms
{
    public interface ILongArmsFleeUtility
    {
        public uint GetLongArmsBasesFree();
        
        public float GetDistanceToClosestTargetToFleeFrom();
        
        public float GetRadiusToFlee();
    }
}