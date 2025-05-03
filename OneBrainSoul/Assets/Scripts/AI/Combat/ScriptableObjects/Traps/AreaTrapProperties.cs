using AI.Combat.ScriptableObjects.AbilityProperties;
using UnityEngine;

namespace AI.Combat.ScriptableObjects.Traps
{
    [CreateAssetMenu(fileName = "Area Trap Properties", menuName = "ScriptableObjects/Traps/Area Trap Properties", order = 0)]
    public class AreaTrapProperties : ScriptableObject
    {
        public AreaAbilityProperties areaAbilityProperties;
    }
}