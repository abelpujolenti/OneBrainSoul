using UnityEngine;

namespace AI.Combat.AbilityAoEColliders
{
    public class AbilityAoEPositions
    {
        private float _timeToReach;
        private Vector3 _originalPosition;
        private Vector3 _rotatedPosition;

        public AbilityAoEPositions(float timeToReach, Vector3 originalPosition)
        {
            _timeToReach = timeToReach;
            _originalPosition = originalPosition;
        }

        public float GetTimeToReach()
        {
            return _timeToReach;
        }

        public void SetWorldPosition(Vector3 currentForward, Vector3 worldPosition)
        {
            RotatePosition(currentForward);

            _rotatedPosition += worldPosition;
        }

        public void RotatePosition(Vector3 currentForward)
        {
            Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, currentForward);
            _rotatedPosition = rotation * _originalPosition;
        }

        public Vector3 GetRotatedPosition()
        {
            return _rotatedPosition;
        }
    }
}