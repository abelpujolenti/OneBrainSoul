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
            
            FloatField(ref aiEnemyProperties.sightMaximumDistance, 0, "Sight Maximum Distance");
            
            FloatField(ref aiEnemyProperties.fov, 0, 360, "FOV");
            
            EditorGUILayout.Space();
            
            LabelField("Rotation");

            EditorGUI.indentLevel++;
            
            FloatField(ref aiEnemyProperties.normalRotationSpeed, 1, "Normal Rotation Speed");

            if (GUI.changed)
            {
                EditorUtility.SetDirty(aiEnemyProperties);
            }
        }
    }
}