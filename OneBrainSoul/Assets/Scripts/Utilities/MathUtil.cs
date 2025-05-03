using UnityEngine;

namespace Utilities
{
    public static class MathUtil
    {
        public static float Map(float value, float originalMin, float originalMax, float newMin, float newMax)
        {
            return newMin + (value - originalMin) * (newMax - newMin) / (originalMax - originalMin);
        }

        public static Vector3 YAxisAngleToVectorXZ(float angle)
        {
            float radians = AngleToRadians(angle);

            return new Vector3(Mathf.Cos(radians), 0, Mathf.Sin(radians));
        }

        public static Vector3 XAxisAngleToVectorYZ(float angle)
        {
            float radians = AngleToRadians(angle);

            return new Vector3(0, Mathf.Sin(radians), Mathf.Cos(radians));
        }

        public static float VectorXZToYAxisAngle(Vector3 position)
        {
            float radians = Mathf.Atan2(position.z, position.x);
            
            float angle = RadiansToAngle(radians) % 360;

            if (angle < 0)
            {
                angle += 360;
            }

            return angle;
        }

        public static float VectorYZToXAxisAngle(Vector3 position)
        {
            float radians = Mathf.Atan2(position.z, position.y);
            
            float angle = RadiansToAngle(radians) % 360;

            if (angle < 0)
            {
                angle += 360;
            }

            return angle;
        }

        public static Vector3 CalculateLinearForceVector(Vector3 targetPosition, Vector3 targetVelocity, Vector3 ownPosition,
            float projectileSpeed, float dispersionRatePer1Meter)
        {
            Vector3 dispersionVector = CalculateDispersion(targetPosition - ownPosition, dispersionRatePer1Meter);

            targetPosition += dispersionVector;
            
            Vector3 vectorToTarget = targetPosition - ownPosition;
            
            if (targetVelocity.magnitude < 0.01f)
            {
                return vectorToTarget.normalized * projectileSpeed;
            }
            
            float a = Vector3.Dot(targetVelocity, targetVelocity) - projectileSpeed * projectileSpeed;
            float b = 2 * Vector3.Dot(targetVelocity, vectorToTarget);
            float c = Vector3.Dot(vectorToTarget, vectorToTarget);

            float discriminant = b * b - 4 * a * c;

            if (discriminant < 0)
            {
                return Vector3.zero;
            }

            float sqrtDiscriminant = Mathf.Sqrt(discriminant);
            float t1 = (-b + sqrtDiscriminant) / (2 * a);
            float t2 = (-b - sqrtDiscriminant) / (2 * a);

            float time = Mathf.Max(t1, t2);
            
            if (time < 0)
            {
                return Vector3.zero;
            }

            Vector3 interceptionPoint = targetPosition + targetVelocity * time;
            return (interceptionPoint - ownPosition).normalized * projectileSpeed;
        }
        
        public static Vector3 CalculateParabolicForceVector(Vector3 targetPosition, Vector3 targetVelocity, Vector3 ownPosition,
            float projectileSpeed, float gravity)
        {
            Vector3 vectorToTarget = targetPosition - ownPosition;
            
            float a = -0.5f * gravity;
            float b = targetVelocity.y;
            float c = vectorToTarget.y;

            float discriminant = b * b - 4 * a * c;

            if (discriminant < 0)
            {
                return Vector3.zero;
            }

            float sqrtDiscriminant = Mathf.Sqrt(discriminant);
            float t1 = (-b + sqrtDiscriminant) / (2 * a);
            float t2 = (-b - sqrtDiscriminant) / (2 * a);

            float time = Mathf.Max(t1, t2);
            
            if (time < 0)
            {
                return Vector3.zero;
            }

            Vector3 forceVector = 
                (vectorToTarget + targetVelocity * time - Vector3.up * (0.5f * gravity * time * time)) / time;

            return forceVector.magnitude > projectileSpeed ? Vector3.zero : forceVector.normalized * projectileSpeed;
        }

        private static Vector3 CalculateDispersion(Vector3 vectorToTarget, float dispersionRatePer1Meter)
        {
            Vector3 randomPerpendicular = Vector3.Cross(vectorToTarget, Vector3.up).normalized;
            
            Quaternion randomRotation = Quaternion.AngleAxis(Random.Range(0f, 360f), vectorToTarget);
            Vector3 randomDeviation = randomRotation * randomPerpendicular;

            return randomDeviation * Random.Range(0, vectorToTarget.magnitude / 100 * dispersionRatePer1Meter);
        }

        public static Vector3 RotateVector(Vector3 vector, Vector3 axis, float degrees)
        {
            Quaternion rotation = Quaternion.AngleAxis(degrees, axis);
            return rotation * vector;
        }

        public static float AngleToRadians(float angle)
        {
            return angle * (Mathf.PI / 180);
        }

        public static float RadiansToAngle(float radians)
        {
            return radians * (180 / Mathf.PI);
        }
    }
}