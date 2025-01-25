using UnityEngine;

namespace AI.Combat.ScriptableObjects
{
    public abstract class AIEnemySpecs : ScriptableObject
    {
        public uint  totalHealth;

        public float agentsPositionRadius;

        public float sightMaximumDistance;
    }
}