using System.Collections.Generic;
using UnityEngine;

namespace ECS.Components.AI.Combat
{
    public class SubThreatGroupComponent
    {
        public float threatWeight;
        
        public Vector3 barycenter;
        
        public float radius;

        public List<uint> enemiesInsideGroup = new List<uint>();
    }
}