using AI.Combat.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Editor.Enemies
{
    [CustomEditor(typeof(TrifaceProperties))]
    public class TrifaceEditor : EnemyEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            TrifaceProperties trifaceProperties = (TrifaceProperties)target;
            
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField("Slam Ability", EditorStyles.boldLabel);

            trifaceProperties.slamAbility =
                (AgentAbility)EditorGUILayout.ObjectField(trifaceProperties.slamAbility, typeof(AgentAbility), false);
            
            EditorGUILayout.EndHorizontal();

            if (!GUI.changed)
            {
                return;
            }
            
            EditorUtility.SetDirty(trifaceProperties);
        }
    }
}