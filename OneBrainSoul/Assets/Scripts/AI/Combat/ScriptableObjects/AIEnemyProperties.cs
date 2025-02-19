using UnityEngine;

namespace AI.Combat.ScriptableObjects
{
    public abstract class AIEnemyProperties : ScriptableObject
    {
        public uint  totalHealth;

        public float agentsPositionRadius;

        public float sightMaximumDistance;

        public float fov;

        public float minimumTimeInvestigatingArea;
        
        public float maximumTimeInvestigatingArea;

        public float minimumTimeInvestigatingAtEstimatedPosition;
        
        public float maximumTimeInvestigatingAtEstimatedPosition;

        public uint maximumHeadYawRotation;
        
        public uint maximumHeadPitchRotation;
        
        public uint minimumHeadPitchRotation;

        public float bodyNormalRotationSpeed;
    }
}