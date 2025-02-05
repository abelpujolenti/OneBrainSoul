namespace Interfaces.AI.Combat
{
    public interface ISlowable
    {
        public void OnReceiveSlow(uint slowID, uint slowPercent);

        public void OnReleaseFromSlow(uint slowID);

        public void OnReceiveSlowOverTime(uint slowID, uint slowPercent, float duration);
        
        public void OnReceiveDecreasingSlow(uint slowID, uint slowPercent, float duration);
    }
}