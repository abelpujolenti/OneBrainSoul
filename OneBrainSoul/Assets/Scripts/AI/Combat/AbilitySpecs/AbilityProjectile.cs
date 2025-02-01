using System;
using UnityEngine;

namespace AI.Combat.AbilitySpecs
{
    [Serializable]
    public class AbilityProjectile
    {
        public GameObject projectilePrefab;
        
        public float projectileSpeed;

        public bool doesVanishOnImpact;
        public bool doesVanishOverTime;
        
        public float timeToVanish;

        public bool doesExplodeOnVanishOverTime;
    }
}