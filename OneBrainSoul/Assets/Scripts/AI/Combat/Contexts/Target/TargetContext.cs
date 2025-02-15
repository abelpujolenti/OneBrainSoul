using UnityEngine;

namespace AI.Combat.Contexts.Target
{
    public class TargetContext
    {
        private float _targetRadius;
        private float _targetHeight;
        private float _distanceToTarget;

        private Vector3 _vectorToTarget;

        private Transform _targetTransform;

        public void SetTargetRadius(float targetRadius)
        {
            _targetRadius = targetRadius;
        }

        public float GetTargetRadius()
        {
            return _targetRadius;
        }

        public void SetTargetHeight(float targetHeight)
        {
            _targetHeight = targetHeight;
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
            _vectorToTarget = vectorToTarget;
            SetDistanceToTarget(_vectorToTarget.magnitude - _targetRadius);
        }

        public Vector3 GetVectorToTarget()
        {
            return _vectorToTarget;
        }

        public void SetTargetTransform(Transform targetTransform, Vector3 ownPosition, float ownHeight)
        {
            _targetTransform = targetTransform;

            Vector3 targetPosition = targetTransform.position;
            targetPosition.y -= _targetHeight / 2;

            Vector3 agentPosition = ownPosition;
            agentPosition.y -= ownHeight / 2;
            
            SetVectorToTarget(targetPosition - agentPosition);
        }

        public Transform GetTargetTransform()
        {
            return _targetTransform;
        }
    }
}