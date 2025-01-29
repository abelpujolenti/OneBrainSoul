using System.Collections.Generic;
using AI.Combat.ScriptableObjects.AbilityConditions;
using UnityEditor;
using UnityEngine;

namespace Editor.Enemies
{
    [CustomEditor(typeof(AbilityConditionOperation))]
    public class AbilityConditionOperationEditor : UnityEditor.Editor
    {
        private Dictionary<OperationsType, string> operationTypes = new Dictionary<OperationsType, string>
        {
            { OperationsType.ADD , "+" },
            { OperationsType.SUBTRACT , "-" },
            { OperationsType.MULTIPLY , "x" },
            { OperationsType.DIVIDE , "/" }
        };
        
        public override void OnInspectorGUI()
        {
            AbilityConditionOperation abilityConditionOperation = (AbilityConditionOperation)target;

            
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(abilityConditionOperation);
            }
        }
    }
}