using UnityEngine;

namespace Interfaces.AI.Combat
{
    public interface ISlowable
    {
        public void OnReceiveSlow(uint slowID, uint slowPercent, Vector3 sourcePosition);

        public void OnReleaseFromSlow(uint slowID);

        public void OnReceiveSlowOverTime(uint slowID, uint slowPercent, float duration, Vector3 sourcePosition);
        
        public void OnReceiveDecreasingSlow(uint slowID, uint slowPercent, float duration, Vector3 sourcePosition);
    }
}