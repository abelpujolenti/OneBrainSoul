using UnityEngine;

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

        public uint maximumHeadYawRotation;
        
        public uint maximumHeadPitchUpRotation;
        
        public uint maximumHeadPitchDownRotation;

        public float headRotationSpeed;

        public float bodyNormalRotationSpeed;

        public GameObject healEffect;

        public Vector3 healRelativePosition;
        public Vector3 healRelativeRotation;
        public Vector3 healRelativeScale;
    }
}