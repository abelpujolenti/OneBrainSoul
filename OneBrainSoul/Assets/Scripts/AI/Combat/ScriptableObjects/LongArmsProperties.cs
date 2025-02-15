using ECS.Entities;
using UnityEngine;

namespace AI.Combat.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Long Arms Properties", menuName = "ScriptableObjects/AI/Combat/Entities/Long Arms Properties", order = 1)]
    public class LongArmsProperties : AIEnemyProperties
    {
        public float rotationSpeedWhileTurningAround;
        
        public float minimumTimeBeforeSettingNewDirection;
        public float maximumTimeBeforeSettingNewDirection;
        
        public uint minimumDegreesToRotateDirection;
        public uint maximumDegreesToRotateDirection;
        
        public float rotationSpeedWhenAcquiringATarget;

        public uint minimumTimesSettingNewDirectionToTurnAround;
        public uint maximumTimesSettingNewDirectionToTurnAround;
        
        public float radiusToFlee;
        public EntityType entitiesToFleeFrom;
        
        public ProjectileAbilityProperties throwRockAbilityProperties;
        public float rotationSpeedWhileCastingThrowRock;
        
        public AreaAbilityProperties clapAboveAbilityProperties;
    }
}