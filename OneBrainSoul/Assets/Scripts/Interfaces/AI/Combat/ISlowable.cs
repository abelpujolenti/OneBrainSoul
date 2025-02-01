namespace Interfaces.AI.Combat
{
    public interface ISlowable
    {
        public void OnReceiveSlow(uint slowPercent);

        public void OnReleaseFromSlow();

        public void OnReceiveSlowOverTime(uint slowPercent, float duration);
        
        public void OnReceiveDecreasingSlow(uint slowPercent, float duration);
    }
}