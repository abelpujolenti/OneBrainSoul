using System;
using UnityEngine;

namespace AI.Combat.AbilitySpecs
{
    public enum AbilityCastType
    {
        OMNIPRESENT,
        NO_PROJECTILE,
        PARABOLA_PROJECTILE,
        STRAIGHT_LINE_PROJECTILE
    }
    
    [Serializable]
    public class AbilityCast
    {
        public float minimumRangeToCast;
        public float maximumRangeToCast;

        public bool doesSpawnInsideCaster;
        public Vector3 relativeSpawnPosition;

        public float timeToCast;

        public float cooldown;

        public float projectileSpeed;
    }
}