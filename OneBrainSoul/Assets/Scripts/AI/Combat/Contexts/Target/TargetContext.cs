using UnityEngine;

namespace AI.Combat.Contexts.Target
{
    public class TargetContext
    {
        private float _targetRadius;
        private float _targetHeight;
        private float _distanceToTarget;

        private Vector3 _vectorToTarget;
        private Vector3 _targetPosition;
        private Vector3 _targetVelocity;

        public void SetTargetProperties(float targetRadius, float targetHeight)
        {
            _targetRadius = targetRadius;
            _targetHeight = targetHeight;
        }

        public float GetTargetRadius()
        {
            return _targetRadius;
        }

        public float GetTargetHeight()
        {
            return _targetHeight;
        }

        public void SetDistanceToTarget(float distanceToTarget)
        {
            _distanceToTarget = distanceToTarget;
        }

        public float GetDistanceToTarget()
        {
            return _distanceToTarget;
        }

        public void SetVectorToTarget(Vector3 vectorToTarget)
        {
            _vectorToTarget = vectorToTarget.normalized * (vectorToTarget.magnitude - _targetRadius);
            SetDistanceToTarget(_vectorToTarget.magnitude);
        }

        public Vector3 GetVectorToTarget()
        {
            return _vectorToTarget;
        }

        public void SetTargetState(Vector3 position, Vector3 velocity)
        {
            _targetPosition = position;
            _targetVelocity = velocity;
        }

        public Vector3 GetTargetPosition()
        {
            return _targetPosition;
        }

        public Vector3 GetTargetVelocity()
        {
            return _targetVelocity;
        }
    }
}