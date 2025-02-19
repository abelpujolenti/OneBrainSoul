using UnityEngine;
using UnityEngine.Serialization;

namespace AI.Combat.ScriptableObjects
{
    public abstract class AIEnemyProperties : ScriptableObject
    {
        public uint  totalHealth;

        public float agentsPositionRadius;

        public float sightMaximumDistance;

        public uint fov;

        public float minimumTimeInvestigatingArea;
        
        public float maximumTimeInvestigatingArea;

        public float minimumTimeInvestigatingAtEstimatedPosition;
        
        public float maximumTimeInvestigatingAtEstimatedPosition;

        public uint maximumHeadYawRotation;
        
        public uint maximumHeadPitchUpRotation;
        
        public uint maximumHeadPitchDownRotation;

        public float headRotationSpeed;

        public float bodyNormalRotationSpeed;
    }
}