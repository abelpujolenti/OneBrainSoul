using AI.Combat.AbilitySpecs;
using UnityEngine;

namespace AI.Combat.ScriptableObjects
{
    [CreateAssetMenu(fileName = "LongArms Properties", menuName = "ScriptableObjects/AI/Combat/Entity/LongArms Properties", order = 1)]
    public class LongArmsSpecs : AIEnemySpecs
    {
        public AbilityEffectType abilityEffectType;

        public AbilityEffect abilityEffect;

    }
}