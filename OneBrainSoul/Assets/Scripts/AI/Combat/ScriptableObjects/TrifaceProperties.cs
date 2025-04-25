using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Serialization;

namespace AI.Combat.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Triface Properties", menuName = "ScriptableObjects/AI/Combat/Entities/Triface Properties", order = 0)]
    public class TrifaceProperties : FreeMobilityEnemyProperties
    {
        public AreaAbilityProperties slamAbilityProperties;

        [FormerlySerializedAs("triggerName")] public string slamAbilityTriggerName;

        public float rotationSpeedWhileCastingSlam;

        public AnimatorController animatorController;
    }
}