using UnityEngine;

namespace AI.Navigation
{
    [CreateAssetMenu(fileName = "NavMesh Agent", menuName = "ScriptableObjects/AI/NavMesh Agent", order = 1)]
    public class NavMeshAgentSpecs : ScriptableObject
    {
        [Min(1f)]
        public float movementSpeed;
    }
}
