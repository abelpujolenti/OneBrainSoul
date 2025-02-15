using AI.Combat.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Editor.Enemies
{
    [CustomEditor(typeof(SendatuProperties))]
    public class SendatuEditor : EnemyEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            SendatuProperties sendatuProperties = (SendatuProperties)target;
            
            EditorGUILayout.Space();
            
            LabelField("Flee");

            EditorGUI.indentLevel++;
            
            EnumFlagsField(ref sendatuProperties.entitiesToFleeFrom, "Entities To Flee From");
            
            FloatField(ref sendatuProperties.radiusToFlee, "Radius To Flee");

            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            LabelField("Abilities");

            EditorGUI.indentLevel++;

            EditorGUI.indentLevel--;

            if (!GUI.changed)
            {
                return;
            }
            
            EditorUtility.SetDirty(sendatuProperties);
        }
    }
}