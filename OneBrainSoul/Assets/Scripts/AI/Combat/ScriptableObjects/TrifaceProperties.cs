using UnityEngine;

namespace AI.Combat.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Triface Properties", menuName = "ScriptableObjects/AI/Combat/Entity/Triface Properties", order = 1)]
    public class TrifaceProperties : AIEnemyProperties
    {
        public AgentAbility slamAbility;
    }
}