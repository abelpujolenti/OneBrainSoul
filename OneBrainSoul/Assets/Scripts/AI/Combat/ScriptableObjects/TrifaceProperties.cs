using UnityEngine;

namespace AI.Combat.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Triface Properties", menuName = "ScriptableObjects/AI/Combat/Entities/Triface Properties", order = 0)]
    public class TrifaceProperties : AIEnemyProperties
    {
        public AreaAbilityProperties slamAbilityProperties;

        public float rotationSpeedWhileCastingSlam;
    }
}