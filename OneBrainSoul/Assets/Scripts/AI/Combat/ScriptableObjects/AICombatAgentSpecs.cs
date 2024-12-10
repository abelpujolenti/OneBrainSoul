using UnityEngine;

namespace AI.Combat.ScriptableObjects
{
    public abstract class AICombatAgentSpecs : ScriptableObject
    {
        public uint  totalHealth;

        public float rivalsPositionRadius;

        public float sightMaximumDistance;
    }
}