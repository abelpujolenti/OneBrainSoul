using UnityEngine;

namespace AI.Combat.ScriptableObjects
{
    public abstract class AIEnemyProperties : ScriptableObject
    {
        public uint  totalHealth;

        public float agentsPositionRadius;

        public float sightMaximumDistance;

        public float fov;

        public float normalRotationSpeed;
    }
}