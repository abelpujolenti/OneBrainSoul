using System;
using UnityEngine;

namespace AI.Combat.AbilitySpecs
{
    [Serializable]
    public class AbilityProjectile
    {
        public GameObject projectilePrefab;

        public Vector3 relativePositionToCaster;

        public bool makesParabola;
        
        public float projectileSpeed;

        public uint instances;

        public float dispersionRatePer1Meter;

        public GameObject objectWithParticleSystem;
        public Vector3 relativePositionForParticles;

        public bool doesVanishOnImpact;
        public bool doesVanishOverTime;
        
        public float timeToVanish;

        public bool doesExplodeOnVanishOverTime;
    }
}