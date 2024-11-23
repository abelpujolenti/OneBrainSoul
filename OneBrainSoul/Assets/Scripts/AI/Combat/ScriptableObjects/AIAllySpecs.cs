using System.Collections.Generic;
using UnityEngine;

namespace AI.Combat.ScriptableObjects
{
    [CreateAssetMenu(fileName = "AI Ally Properties", menuName = "ScriptableObjects/AI/Combat/Entity/AI Ally Properties", order = 0)]
    public class AIAllySpecs : AICombatAgentSpecs
    {
        public readonly AIAgentType aiAgentType = AIAgentType.ALLY;
        
        public readonly float moralWeight = 3;
        public float alertRadius;
        public float safetyRadius;

        [SerializeField] public List<AIAllyAttack> aiAttacks;
    }
}