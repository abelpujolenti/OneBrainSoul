using AI.Combat.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Editor.Enemies
{
    [CustomEditor(typeof(TrifaceSpecs))]
    public class TrifaceEditor : EnemyEditor
    {
        private UnityEditor.Editor slamEditor;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            TrifaceSpecs trifaceSpecs = (TrifaceSpecs)target;

            slamEditor = CreateEditor(trifaceSpecs.SlamAbility);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Slam", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            slamEditor.OnInspectorGUI();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(trifaceSpecs);
            }
        }
    }
}