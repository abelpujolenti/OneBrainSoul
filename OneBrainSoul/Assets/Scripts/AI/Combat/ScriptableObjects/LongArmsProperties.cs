using ECS.Entities;
using UnityEngine;

namespace AI.Combat.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Long Arms Properties", menuName = "ScriptableObjects/AI/Combat/Entities/Long Arms Properties", order = 1)]
    public class LongArmsProperties : TeleportMobilityEnemyProperties
    {
        public float bodyRotationSpeedWhileTurningAround;
        
        public float minimumTimeBeforeSettingNewDirection;
        public float maximumTimeBeforeSettingNewDirection;
        
        public uint bodyMinimumDegreesToRotateDirection;
        public uint bodyMaximumDegreesToRotateDirection;
        
        public float bodyRotationSpeedWhenAcquiringATarget;

        public uint minimumTimesSettingNewDirectionToTurnAround;
        public uint maximumTimesSettingNewDirectionToTurnAround;
        
        public float radiusToFlee;
        public EntityType entitiesToFleeFrom;
        
        public ProjectileAbilityProperties throwRockAbilityProperties;
        public float bodyNotationSpeedWhileCastingThrowRock;
        
        public AreaAbilityProperties clapAboveAbilityProperties;
    }
}