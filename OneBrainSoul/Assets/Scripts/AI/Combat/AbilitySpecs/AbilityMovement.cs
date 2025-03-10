using System;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Combat.AbilitySpecs
{
    [Serializable]
    public class AbilityMovement
    {
        public bool makesBezierCurves;
        
        public List<Vector3> positions = new List<Vector3>();

        public List<float> timeBetweenPositions = new List<float>();
    }
}