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
            
            UintField(ref trifaceProperties.maximumHeadYawRotation, 0, "Maximum Head Yaw Rotation");
            
            UintField(ref trifaceProperties.maximumHeadPitchUpRotation, 0, "Maximum Head Pitch Up Rotation");
            
            UintField(ref trifaceProperties.maximumHeadPitchDownRotation, 0, "Maximum Head Pitch Down Rotation");
            
            FloatField(ref trifaceProperties.headRotationSpeed, 1, "Head Rotation Speed");
            
            FloatField(ref trifaceProperties.bodyNormalRotationSpeed, 1, "Body Normal Rotation Speed");

            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            LabelField("Abilities");

            EditorGUI.indentLevel++;
            
            ObjectField(ref trifaceProperties.slamAbilityProperties, "Slam Ability");
            
            FloatField(ref trifaceProperties.rotationSpeedWhileCastingSlam, 0, "Rotation Speed While Casting Slam");

            EditorGUI.indentLevel--;

            if (!GUI.changed)
            {
                return;
            }
            
            EditorUtility.SetDirty(trifaceProperties);
        }
    }
}