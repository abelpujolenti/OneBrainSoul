using System.Collections.Generic;
using UnityEngine;

namespace AI.Combat.ScriptableObjects.AbilityConditions
{
    [CreateAssetMenu(fileName = "Ability Utility Condition", menuName = "ScriptableObjects/AI/Combat/Ability Condition/Statement", order = 0)]
    public class AbilityConditionStatement : ScriptableObject
    {
        public List<AbilityConditionComparison> abilityConditionComparisons = new List<AbilityConditionComparison>();

        public List<LogicGatesType> logicGates = new List<LogicGatesType>();
        
        public float result;

        private void OnEnable()
        {
            if (abilityConditionComparisons.Count < 2)
            {
                for (int i = abilityConditionComparisons.Count; i < 2; i++)
                {
                    abilityConditionComparisons.Add(CreateInstance<AbilityConditionComparison>());
                }
                
                logicGates.Add(LogicGatesType.AND);
            }
        }
    }
}