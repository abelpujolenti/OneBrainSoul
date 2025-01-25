using System;

namespace AI.Combat.AbilitySpecs
{
    public enum AbilityProjectileType
    {
        VANISH_ON_IMPACT,
        VANISH_OVER_TIME
    }

    [Serializable]
    public class AbilityProjectile
    {
        public float timeToVanish;
    }
}