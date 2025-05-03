using AI.Combat.ScriptableObjects.Enemies;
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
            
            EditorGUILayout.Space();
            
            LabelField("VFX");

            EditorGUI.indentLevel++;
            
            ObjectField(ref sendatuProperties.healEffect, "Heal Effect");

            Vector3Field(ref sendatuProperties.healRelativePosition, "Heal Position");
            Vector3Field(ref sendatuProperties.healRelativeRotation, "Heal Rotation");
            Vector3Field(ref sendatuProperties.healRelativeScale, "Heal Scale");

            EditorGUI.indentLevel--;
            
            LabelField("Animation");

            EditorGUI.indentLevel++;
            
            ObjectField(ref sendatuProperties.animatorController, "Animator Controller");
            
            EditorGUI.indentLevel--;

            if (!GUI.changed)
            {
                return;
            }
            
            EditorUtility.SetDirty(sendatuProperties);
        }
    }
}