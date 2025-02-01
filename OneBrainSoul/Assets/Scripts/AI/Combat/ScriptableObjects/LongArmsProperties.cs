using AI.Combat.AbilitySpecs;
using UnityEngine;
using UnityEngine.Serialization;

namespace AI.Combat.ScriptableObjects
{
    [CreateAssetMenu(fileName = "LongArms Properties", menuName = "ScriptableObjects/AI/Combat/Entity/LongArms Properties", order = 1)]
    public class LongArmsProperties : AIEnemyProperties
    {
        [FormerlySerializedAs("abilityEffectType")] public AbilityEffectOnHealthType abilityEffectOnHealthType;

        public AbilityEffect abilityEffect;

    }
}