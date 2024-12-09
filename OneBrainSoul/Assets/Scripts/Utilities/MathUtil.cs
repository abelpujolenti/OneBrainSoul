using UnityEngine;

namespace Utilities
{
    public static class MathUtil
    {
        public static float Map(float value, float originalMin, float originalMax, float newMin, float newMax)
        {
            return newMin + (value - originalMin) * (newMax - newMin) / (originalMax - originalMin);
        }

        public static Vector3 AngleToVector(float angle)
        {
            float radians = AngleToRadians(angle);

            return new Vector3(Mathf.Sin(radians), 0, -Mathf.Cos(radians));
        }

        public static float VectorToAngle(Vector3 position)
        {
            float radians = Mathf.Atan2(position.z, position.x);
            
            float angle = MathUtil.RadiansToAngle(radians) % 360;

            if (angle < 0)
            {
                angle += 360;
            }

            return angle;
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