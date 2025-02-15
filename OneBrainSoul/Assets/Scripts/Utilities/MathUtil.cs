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