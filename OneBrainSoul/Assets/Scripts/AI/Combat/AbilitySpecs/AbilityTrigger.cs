using System;

namespace AI.Combat.AbilitySpecs
{
    [Serializable]
    public class AbilityTrigger
    {
        public bool doesEffectOnStart;
        public bool doesEffectOnTheDuration;
        public bool doesEffectOnEnd;
    }
}