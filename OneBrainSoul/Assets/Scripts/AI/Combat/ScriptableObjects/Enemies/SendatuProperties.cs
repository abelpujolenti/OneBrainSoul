using ECS.Entities;
using UnityEngine;

namespace AI.Combat.ScriptableObjects.Enemies
{
    [CreateAssetMenu(fileName = "Sendatu Properties", menuName = "ScriptableObjects/AI/Combat/Entities/Sendatu Properties", order = 2)]
    public class SendatuProperties : AIEnemyProperties
    {
        public float radiusToFlee;
        public EntityType entitiesToFleeFrom;
        
        public RuntimeAnimatorController animatorController;
    }
}