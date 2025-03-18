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
            
            LabelField("Rotation");

            EditorGUI.indentLevel++;
            
            FloatField(ref trifaceProperties.bodyNormalRotationSpeed, 1, "Body Normal Rotation Speed");

            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            LabelField("Abilities");

            EditorGUI.indentLevel++;
            
            ObjectField(ref trifaceProperties.slamAbilityProperties, "Slam Ability");
            
            FloatField(ref trifaceProperties.rotationSpeedWhileCastingSlam, 0, "Rotation Speed While Casting Slam");

            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            LabelField("VFX");

            EditorGUI.indentLevel++;
            
            ObjectField(ref trifaceProperties.healEffect, "Heal Effect");

            Vector3Field(ref trifaceProperties.healRelativePosition, "Heal Position");
            Vector3Field(ref trifaceProperties.healRelativeRotation, "Heal Rotation");
            Vector3Field(ref trifaceProperties.healRelativeScale, "Heal Scale");

            EditorGUI.indentLevel--;

            if (!GUI.changed)
            {
                return;
            }
            
            EditorUtility.SetDirty(trifaceProperties);
        }
    }
}