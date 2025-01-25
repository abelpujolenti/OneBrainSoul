using AI.Combat.AbilitySpecs;
using UnityEngine;

namespace AI.Combat.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Triface Properties", menuName = "ScriptableObjects/AI/Combat/Entity/Triface Properties", order = 1)]
    public class TrifaceSpecs : AIEnemySpecs
    {
        public AbilityTarget slamTarget;
        
        public AbilityEffectType slamEffectType;
        
        public AbilityEffect slamEffect;

        public AbilityCastType slamCastType;

        public AbilityCast slamCast;

        public AbilityAoEType slamAoEType;

        public AbilityAoE slamAoE;
    }
}