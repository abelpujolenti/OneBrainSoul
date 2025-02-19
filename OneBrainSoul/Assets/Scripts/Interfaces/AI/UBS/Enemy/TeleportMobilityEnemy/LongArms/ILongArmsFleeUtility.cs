namespace Interfaces.AI.UBS.Enemy.TeleportMobilityEnemy.LongArms
{
    public interface ILongArmsFleeUtility
    {
        public uint GetLongArmsBasesFree();
        
        public float GetDistanceToClosestTargetToFleeFrom();
        
        public float GetRadiusToFlee();
    }
}