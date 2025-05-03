using AI.Combat.ScriptableObjects.AbilityProperties;
using UnityEngine;

namespace AI.Combat.ScriptableObjects.Traps
{
    [CreateAssetMenu(fileName = "Projectile Trap Properties", menuName = "ScriptableObjects/Traps/Projectile Trap Properties", order = 1)]
    public class ProjectileTrapProperties : ScriptableObject
    {
        public ProjectileAbilityProperties projectileAbilityProperties;
    }
}