using System.Collections.Generic;
using UnityEngine;

namespace AI.Combat.ScriptableObjects.AbilityConditions
{
    [CreateAssetMenu(fileName = "Ability Utility Operation", menuName = "ScriptableObjects/AI/Combat/Ability Condition/Operation", order = 2)]
    public class AbilityConditionOperation : ScriptableObject
    {
        public List<OperationsType> operationsTypes = new List<OperationsType>();
    }
}