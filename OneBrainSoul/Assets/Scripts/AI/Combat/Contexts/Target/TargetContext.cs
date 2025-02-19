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

        private bool _canSeeTarget;
        private LoseSightOfTargetCause _loseSightOfTargetCause;

        public void SetTargetProperties(float targetRadius, float targetHeight)
        {
            _targetRadius = targetRadius;
            _targetHeight = targetHeight;
            _canSeeTarget = true;
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

        public bool CanSeeTarget()
        {
            return _canSeeTarget;
        }

        public void OnLoseSightOfTarget()
        {
            _canSeeTarget = false;
        }

        public void SetLoseSightOfTargetCause(LoseSightOfTargetCause loseSightOfTargetCause)
        {
            _loseSightOfTargetCause = loseSightOfTargetCause;
        }

        public LoseSightOfTargetCause GetLoseSightOfTargetCause()
        {
            return _loseSightOfTargetCause;
        }
    }
}