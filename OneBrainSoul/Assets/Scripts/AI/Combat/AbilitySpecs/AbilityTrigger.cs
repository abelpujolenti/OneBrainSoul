using System;

namespace AI.Combat.AbilitySpecs
{
    [Serializable]
    public class AbilityTrigger
    {
        public bool hasAnEffectOnStart;
        public bool hasAnEffectOnTheDuration;
        public bool hasAnEffectOnEnd;
    }
}