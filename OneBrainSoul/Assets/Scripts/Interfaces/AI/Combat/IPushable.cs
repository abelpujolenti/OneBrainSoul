using UnityEngine;

namespace Interfaces.AI.Combat
{
    public interface IPushable
    {
        public void OnReceivePush(Vector3 forceDirection, float forceStrength);
    }
}