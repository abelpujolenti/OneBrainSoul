using System.Collections.Generic;
using AI.Combat.ScriptableObjects.AbilityConditions;
using UnityEditor;
using UnityEngine;

namespace Editor.Enemies
{
    [CustomEditor(typeof(AbilityConditionComparison))]
    public class AbilityConditionComparisonEditor : UnityEditor.Editor
    {
        private Dictionary<ComparisonType, string> comparisonTypes = new Dictionary<ComparisonType, string>
        {
            { ComparisonType.GREATER_THAN , ">" },
            { ComparisonType.GREATER_EQUAL_OF , ">=" },
            { ComparisonType.LESS_THAN , "<" },
            { ComparisonType.LESS_EQUAL_OF , "<=" },
            { ComparisonType.EQUAL , "==" },
            { ComparisonType.NOT_EQUAL , "!=" },
        };
        
        public override void OnInspectorGUI()
        {
            AbilityConditionComparison abilityConditionComparison = (AbilityConditionComparison)target;

            for (int i = 0; i < abilityConditionComparison.abilityConditionOperations.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                abilityConditionComparison.abilityConditionOperations[i] =
                    (AbilityConditionOperation)EditorGUILayout.ObjectField("",
                        abilityConditionComparison.abilityConditionOperations[i], typeof(AbilityConditionOperation), false);

                if (abilityConditionComparison.abilityConditionOperations.Count > 2 &&
                    GUILayout.Button("-", GUILayout.Width(25)))
                {
                    abilityConditionComparison.abilityConditionOperations.RemoveAt(i);

                    if (i == abilityConditionComparison.comparisonTypes.Count)
                    {
                        abilityConditionComparison.comparisonTypes.RemoveAt(i - 1);
                        continue;
                    }
                    
                    abilityConditionComparison.comparisonTypes.RemoveAt(i);

                    if (i - 1 < 0)
                    {
                        continue;
                    }

                    abilityConditionComparison.comparisonTypes[i - 1] = ComparisonType.GREATER_THAN;
                }
                
                
                EditorGUILayout.EndHorizontal();

                if (i < abilityConditionComparison.abilityConditionOperations.Count - 1)
                {
                    abilityConditionComparison.comparisonTypes[i] =
                        (ComparisonType)EditorGUILayout.EnumPopup("", abilityConditionComparison.comparisonTypes[i]);
                }
            }

            if (GUILayout.Button("+"))
            {
                abilityConditionComparison.abilityConditionOperations.Add(CreateInstance<AbilityConditionOperation>());
                abilityConditionComparison.comparisonTypes.Add(ComparisonType.GREATER_THAN);
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(abilityConditionComparison);
            }
        }
    }
}