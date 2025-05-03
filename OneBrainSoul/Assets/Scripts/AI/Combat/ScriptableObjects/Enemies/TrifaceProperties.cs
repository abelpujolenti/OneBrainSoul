using AI.Combat.ScriptableObjects.AbilityProperties;
using UnityEngine;

namespace AI.Combat.ScriptableObjects.Enemies
{
    [CreateAssetMenu(fileName = "Triface Properties", menuName = "ScriptableObjects/AI/Combat/Entities/Triface Properties", order = 0)]
    public class TrifaceProperties : FreeMobilityEnemyProperties
    {
        public AreaAbilityProperties slamAbilityProperties;

        public string slamAbilityTriggerName;

        public float rotationSpeedWhileCastingSlam;
    }
}