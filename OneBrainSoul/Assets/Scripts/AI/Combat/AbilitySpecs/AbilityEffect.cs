using System;

namespace AI.Combat.AbilitySpecs
{
    public enum AbilityEffectType
    {
        DIRECT_DAMAGE,
        DAMAGE_PER_TICKS,
        DIRECT_HEAL,
        HEAL_PER_TICKS,
        SLOW
    }

    [Serializable]
    public class AbilityEffect
    {
        public uint value;

        public float duration;
    }
}