using System.Collections.Generic;
using UnityEngine;

namespace AI.Combat.ScriptableObjects.AbilityConditions
{
    [CreateAssetMenu(fileName = "Ability Utility Comparison", menuName = "ScriptableObjects/AI/Combat/Ability Condition/Comparison", order = 1)]
    public class AbilityConditionComparison : ScriptableObject
    {
        public List<AbilityConditionOperation> abilityConditionOperations = new List<AbilityConditionOperation>();

        public List<ComparisonType> comparisonTypes = new List<ComparisonType>();

        private void OnEnable()
        {
            if (abilityConditionOperations.Count < 2)
            {
                for (int i = abilityConditionOperations.Count; i < 2; i++)
                {
                    abilityConditionOperations.Add(CreateInstance<AbilityConditionOperation>());
                }
                
                comparisonTypes.Add(ComparisonType.GREATER_THAN);
            }
        }
    }
}