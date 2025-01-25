using System;

namespace AI.Combat.AbilitySpecs
{
    public enum AbilityEffectType
    {
        DIRECT_DAMAGE,
        DAMAGE_PER_DOTS,
        DIRECT_HEAL,
        HEAL_PER_DOTS,
        SLOW
    }

    [Serializable]
    public class AbilityEffect
    {
        public float damage;

        public float totalDamage;
        public float damageDuration;

        public float heal;

        public float totalHeal;
        public float healDuration;

        public float slowStrength;

        public float GetDamageValue()
        {
            return damage;
        }

        public float GetTotalDamageValue()
        {
            return totalDamage;
        }

        public float GetDamageDuration()
        {
            return damageDuration;
        }

        public float GetHealValue()
        {
            return heal;
        }

        public float GetTotalHealValue()
        {
            return totalHeal;
        }

        public float GetHealDuration()
        {
            return healDuration;
        }

        public float GetSlowStrength()
        {
            return slowStrength;
        }
    }
}