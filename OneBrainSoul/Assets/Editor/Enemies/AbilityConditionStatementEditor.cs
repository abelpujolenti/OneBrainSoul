using System;
using System.Collections.Generic;
using AI.Combat.ScriptableObjects.AbilityConditions;
using UnityEditor;
using UnityEngine;

namespace Editor.Enemies
{
    [CustomEditor(typeof(AbilityConditionStatement))]
    public class AbilityConditionStatementEditor : UnityEditor.Editor
    {
        private Dictionary<LogicGatesType, string> logicGates = new Dictionary<LogicGatesType, string>
        {
            { LogicGatesType.AND , "AND" },
            { LogicGatesType.OR , "OR" },
            { LogicGatesType.XOR , "XOR" }
        };

        public override void OnInspectorGUI()
        {
            AbilityConditionStatement abilityConditionStatement = (AbilityConditionStatement)target;

            for (int i = 0; i < abilityConditionStatement.abilityConditionComparisons.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                
                abilityConditionStatement.abilityConditionComparisons[i] =
                    (AbilityConditionComparison)EditorGUILayout.ObjectField("",
                        abilityConditionStatement.abilityConditionComparisons[i], typeof(AbilityConditionComparison), false);
                
                if (abilityConditionStatement.abilityConditionComparisons.Count > 2 && 
                    GUILayout.Button("-", GUILayout.Width(25)))
                {
                    abilityConditionStatement.abilityConditionComparisons.RemoveAt(i);

                    if (i == abilityConditionStatement.logicGates.Count)
                    {
                        abilityConditionStatement.logicGates.RemoveAt(i - 1);
                        continue;
                    }
                    abilityConditionStatement.logicGates.RemoveAt(i);

                    if (i - 1 < 0)
                    {
                        continue;
                    }

                    abilityConditionStatement.logicGates[i - 1] = LogicGatesType.AND;
                }
                
                EditorGUILayout.EndHorizontal();
                
                if (i < abilityConditionStatement.abilityConditionComparisons.Count - 1)
                {
                    abilityConditionStatement.logicGates[i] = 
                        (LogicGatesType)EditorGUILayout.EnumPopup("", abilityConditionStatement.logicGates[i]);
                }
            }
            
            if (GUILayout.Button("+"))
            {
                abilityConditionStatement.abilityConditionComparisons.Add(CreateInstance<AbilityConditionComparison>());
                abilityConditionStatement.logicGates.Add(LogicGatesType.AND);
            }
            
            abilityConditionStatement.result =
                EditorGUILayout.FloatField("Result", abilityConditionStatement.result);

            abilityConditionStatement.result = Mathf.Max(abilityConditionStatement.result, 0);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(abilityConditionStatement);
            }
        }
    }
}