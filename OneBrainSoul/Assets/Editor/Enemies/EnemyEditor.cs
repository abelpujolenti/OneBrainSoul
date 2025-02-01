using AI.Combat.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Editor.Enemies
{
    [CustomEditor(typeof(AIEnemyProperties))]
    public class EnemyEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            AIEnemyProperties aiEnemyProperties = (AIEnemyProperties)target;

            aiEnemyProperties.totalHealth = (uint)EditorGUILayout.IntField("Total Health", (int)aiEnemyProperties.totalHealth);

            aiEnemyProperties.agentsPositionRadius =
                EditorGUILayout.FloatField("Agents Position Radius", aiEnemyProperties.agentsPositionRadius);

            aiEnemyProperties.sightMaximumDistance =
                EditorGUILayout.FloatField("Sight Maximum Distance", aiEnemyProperties.sightMaximumDistance);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(aiEnemyProperties);
            }
        }
    }
}