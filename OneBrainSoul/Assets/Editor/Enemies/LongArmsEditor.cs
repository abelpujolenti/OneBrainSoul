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
            
            FloatField(ref longArmsProperties.minimumTimeBeforeSettingNewDirection, 0, "Minimum Time Before Setting New Direction");
            
            FloatField(ref longArmsProperties.maximumTimeBeforeSettingNewDirection, 
                longArmsProperties.minimumTimeBeforeSettingNewDirection, "Maximum Time Before Setting New Direction");
            
            UintField(ref longArmsProperties.minimumDegreesToRotateDirection, 0, "Minimum Degrees To Rotate Direction At Once");
            
            UintField(ref longArmsProperties.maximumDegreesToRotateDirection, longArmsProperties.minimumDegreesToRotateDirection, 
                "Maximum Degrees To Rotate Direction At Once");
            
            FloatField(ref longArmsProperties.rotationSpeedWhenAcquiringATarget, longArmsProperties.normalRotationSpeed, 
                "Rotation Speed When Acquiring a Target");

            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            LabelField("Turn Around");

            EditorGUI.indentLevel++;
            
            UintField(ref longArmsProperties.minimumTimesSettingNewDirectionToTurnAround, 0, "Minimum Times Setting New Direction To Turn Around");
            
            UintField(ref longArmsProperties.maximumTimesSettingNewDirectionToTurnAround, longArmsProperties.minimumTimesSettingNewDirectionToTurnAround, 
                "Maximum Times Setting New Direction To Turn Around");
            
            FloatField(ref longArmsProperties.rotationSpeedWhileTurningAround, longArmsProperties.normalRotationSpeed, 
                "Rotation Speed While Turning Around");

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
            
            FloatField(ref longArmsProperties.rotationSpeedWhileCastingThrowRock, 0.1f, "Rotation Speed While Casting Throw Rock");
            
            EditorGUILayout.Space();
            
            ObjectField(ref longArmsProperties.clapAboveAbilityProperties, "Clap Above Ability");

            EditorGUI.indentLevel--;

            if (!GUI.changed)
            {
                return;
            }
            
            EditorUtility.SetDirty(longArmsProperties);
        }
    }
}