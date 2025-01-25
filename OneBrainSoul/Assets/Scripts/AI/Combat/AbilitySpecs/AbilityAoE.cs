using System;
using UnityEngine;

namespace AI.Combat.AbilitySpecs
{
    public enum AbilityAoEType
    {
        RECTANGLE_AREA,
        CIRCLE_AREA,
        CONE_AREA
    }
    
    [Serializable]
    public class AbilityAoE 
    {
        public Vector3 direction;

        public float width;
        public float height;
        public float length;
        
        public float radius;

        public float degrees;

        public float GetLength()
        {
            return length;
        }
        
        public float GetWideness()
        {
            return width;
        }

        public float GetRadius()
        {
            return radius;
        }

        public Vector3 GetDirection()
        {
            return direction;
        }

        public float GetDegrees()
        {
            return degrees;
        }
    }
}