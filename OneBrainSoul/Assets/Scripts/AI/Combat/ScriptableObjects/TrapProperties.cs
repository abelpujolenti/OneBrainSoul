using UnityEngine;

namespace AI.Combat.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Trap Properties", menuName = "ScriptableObjects/Trap Properties", order = 1)]
    public class TrapProperties : ScriptableObject
    {
        public AreaAbilityProperties trapAbilityProperties;
    }
}