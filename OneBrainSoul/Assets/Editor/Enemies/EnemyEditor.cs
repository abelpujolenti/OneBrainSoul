using AI.Combat.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Editor.Enemies
{
    [CustomEditor(typeof(AIEnemyProperties))]
    public class EnemyEditor : MyEditor
    {
        public override void OnInspectorGUI()
        {
            AIEnemyProperties aiEnemyProperties = (AIEnemyProperties)target;
            
            UintField(ref aiEnemyProperties.totalHealth, 0, "Total Health");
            
            FloatField(ref aiEnemyProperties.agentsPositionRadius, 0, "Agents Position Radius");
            
            EditorGUILayout.Space();

            LabelField("Targetting");
            
            EditorGUI.indentLevel++;
            
            FloatField(ref aiEnemyProperties.sightMaximumDistance, 0, "Sight Maximum Distance");
            
            UintField(ref aiEnemyProperties.fov, 0, 360, "FOV");
            
            FloatField(ref aiEnemyProperties.minimumTimeInvestigatingArea, 0, "Minimum Time Investigating Area");
            
            FloatField(ref aiEnemyProperties.maximumTimeInvestigatingArea, 0, "Maximum Time Investigating Area");
            
            FloatField(ref aiEnemyProperties.minimumTimeInvestigatingAtEstimatedPosition, 
                aiEnemyProperties.minimumTimeInvestigatingArea, "Minimum Time Investigating At Estimated Position");
            
            FloatField(ref aiEnemyProperties.maximumTimeInvestigatingAtEstimatedPosition, 
                aiEnemyProperties.maximumTimeInvestigatingArea, "Maximum Time Investigating At Estimated Position");
            
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(aiEnemyProperties);
            }
        }
    }
}