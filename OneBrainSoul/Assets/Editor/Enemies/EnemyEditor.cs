using AI.Combat.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Editor.Enemies
{
    [CustomEditor(typeof(AIEnemySpecs))]
    public class EnemyEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            AIEnemySpecs aiEnemySpecs = (AIEnemySpecs)target;

            aiEnemySpecs.totalHealth = (uint)EditorGUILayout.IntField("Total Health", (int)aiEnemySpecs.totalHealth);

            aiEnemySpecs.agentsPositionRadius =
                EditorGUILayout.FloatField("Agents Position Radius", aiEnemySpecs.agentsPositionRadius);

            aiEnemySpecs.sightMaximumDistance =
                EditorGUILayout.FloatField("Sight Maximum Distance", aiEnemySpecs.sightMaximumDistance);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(aiEnemySpecs);
            }
        }
    }
}