using System;
using UnityEngine;

namespace AI.Combat.AbilitySpecs
{
    [Serializable]
    public class AbilityProjectile
    {
        public GameObject projectilePrefab;

        public Vector3 relativePositionToCaster;
        
        public float projectileSpeed;
        
        public float timeToVanish;

        public bool doesExplodeOnVanishOverTime;

        public uint instances;

        public float dispersionRatePer1Meter;

        public GameObject objectWithParticleSystem;
        public Vector3 relativePositionForParticles;
    }
}