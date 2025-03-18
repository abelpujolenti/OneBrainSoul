using UnityEngine;

namespace Interfaces.AI.Combat
{
    public interface IPushable
    {
        public void OnReceivePushFromCenter(Vector3 centerPosition, Vector3 forceDirection, float forceStrength,
            Vector3 casterToTargetDirection, ForceMode forceMode);

        public void OnReceivePushInADirection(Vector3 colliderForwardVector, Vector3 forceDirection,
            float forceStrength, Vector3 casterToTargetDirection, ForceMode forceMode);
    }
}