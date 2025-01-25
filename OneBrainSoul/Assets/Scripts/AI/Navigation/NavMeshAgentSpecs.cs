using UnityEngine;

namespace AI.Navigation
{
    [CreateAssetMenu(fileName = "NavMesh Agent", menuName = "ScriptableObjects/AI/NavMesh Agent", order = 1)]
    public class NavMeshAgentSpecs : ScriptableObject
    {
        [Min(0.05f)]
        public float radius;

        [Min(1f)]
        public float movementSpeed;

        [Min(1f)] 
        public float rotationSpeed;
    }
}
