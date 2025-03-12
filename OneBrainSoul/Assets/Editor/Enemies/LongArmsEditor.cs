using AI.Combat.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Editor.Enemies
{
    [CustomEditor(typeof(LongArmsProperties))]
    public class LongArmsEditor : EnemyEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            LongArmsProperties longArmsProperties = (LongArmsProperties)target;
            
            LabelField("Rotation");

            EditorGUI.indentLevel++;
            
            UintField(ref longArmsProperties.maximumHeadYawRotation, 0, "Maximum Head Yaw Rotation");
            
            UintField(ref longArmsProperties.maximumHeadPitchUpRotation, 0, "Maximum Head Pitch Up Rotation");
            
            UintField(ref longArmsProperties.maximumHeadPitchDownRotation, 0, "Maximum Head Pitch Down Rotation");
            
            FloatField(ref longArmsProperties.headRotationSpeed, 1, "Head Rotation Speed");
            
            FloatField(ref longArmsProperties.bodyNormalRotationSpeed, 1, "Body Normal Rotation Speed");
            
            FloatField(ref longArmsProperties.minimumTimeBeforeSettingNewDirection, 0, "Minimum Time Before Setting New Direction");
            
            FloatField(ref longArmsProperties.maximumTimeBeforeSettingNewDirection, 
                longArmsProperties.minimumTimeBeforeSettingNewDirection, "Maximum Time Before Setting New Direction");
            
            UintField(ref longArmsProperties.bodyMinimumDegreesToRotateDirection, 0, "Body Minimum Degrees To Rotate Direction At Once");
            
            UintField(ref longArmsProperties.bodyMaximumDegreesToRotateDirection, longArmsProperties.bodyMinimumDegreesToRotateDirection, 
                "Body Maximum Degrees To Rotate Direction At Once");
            
            FloatField(ref longArmsProperties.bodyRotationSpeedWhenAcquiringATarget, longArmsProperties.bodyNormalRotationSpeed, 
                "Body Rotation Speed When Acquiring a Target");

            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            LabelField("Turn Around");

            EditorGUI.indentLevel++;
            
            UintField(ref longArmsProperties.minimumTimesSettingNewDirectionToTurnAround, 0, "Minimum Times Setting New Direction To Turn Around");
            
            UintField(ref longArmsProperties.maximumTimesSettingNewDirectionToTurnAround, longArmsProperties.minimumTimesSettingNewDirectionToTurnAround, 
                "Maximum Times Setting New Direction To Turn Around");
            
            FloatField(ref longArmsProperties.bodyRotationSpeedWhileTurningAround, longArmsProperties.bodyNormalRotationSpeed, 
                "Body Rotation Speed While Turning Around");

            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            LabelField("Flee");

            EditorGUI.indentLevel++;
            
            EnumFlagsField(ref longArmsProperties.entitiesToFleeFrom, "Entities To Flee From");
            
            FloatField(ref longArmsProperties.radiusToFlee, 0, "Radius To Flee");

            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            LabelField("Abilities");

            EditorGUI.indentLevel++;
            
            ObjectField(ref longArmsProperties.throwRockAbilityProperties, "Throw Rock Ability");
            
            FloatField(ref longArmsProperties.bodyNotationSpeedWhileCastingThrowRock, 0.1f, "Rotation Speed While Casting Throw Rock");
            
            EditorGUILayout.Space();
            
            ObjectField(ref longArmsProperties.clapAboveAbilityProperties, "Clap Above Ability");

            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            LabelField("VFX");

            EditorGUI.indentLevel++;
            
            ObjectField(ref longArmsProperties.healEffect, "Heal Effect");

            Vector3Field(ref longArmsProperties.healRelativePosition, "Heal Position");
            Vector3Field(ref longArmsProperties.healRelativeRotation, "Heal Rotation");
            Vector3Field(ref longArmsProperties.healRelativeScale, "Heal Scale");

            EditorGUI.indentLevel--;

            if (!GUI.changed)
            {
                return;
            }
            
            EditorUtility.SetDirty(longArmsProperties);
        }
    }
}