using UnityEngine;

namespace AI.Combat.ScriptableObjects
{
    public abstract class AIEnemyProperties : ScriptableObject
    {
        public uint  totalHealth;

        public float agentsPositionRadius;

        public float minimumTimeInvestigatingArea;
        
        public float maximumTimeInvestigatingArea;

        public float bodyNormalRotationSpeed;

        public GameObject healEffect;

        public Vector3 healRelativePosition;
        public Vector3 healRelativeRotation;
        public Vector3 healRelativeScale;
    }
}