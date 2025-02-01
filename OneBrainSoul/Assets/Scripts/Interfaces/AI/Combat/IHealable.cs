namespace Interfaces.AI.Combat
{
    public interface IHealable
    {
        public void OnReceiveHeal(uint healValue);

        public void OnReceiveHealOverTime(uint healValue, float duration);
    }
}