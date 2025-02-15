using AI.Combat.AbilitySpecs;
using AI.Combat.ScriptableObjects;
using ECS.Entities;
using UnityEditor;
using UnityEngine;

namespace Editor.Enemies
{
    [CustomEditor(typeof(AreaAbilityProperties))]
    public class AreaAbilityEditor : MyEditor
    {
        private bool isAbilityTargetFoldoutOpen = true;
        private bool isAbilityCastFoldoutOpen = true;
        private bool isAbilityTriggersFoldoutOpen = true;
        private bool isAbilityEffectOnStartFoldoutOpen = true;
        private bool isAbilityEffectOnTheDurationFoldoutOpen = true;
        private bool isAbilityEffectOnEndFoldoutOpen = true;
        private bool isAbilityAoEFoldoutOpen = true;
        
        public bool shapeIsCustom;
        public bool unifyScales;

        public override void OnInspectorGUI()
        {
            InitializeStyles();
            
            AreaAbilityProperties areaAbilityProperties = (AreaAbilityProperties)target;
            
            AbilityTarget(areaAbilityProperties);

            AbilityCast(areaAbilityProperties);

            if (!GUI.changed)
            {
                return;
            }

            EditorUtility.SetDirty(areaAbilityProperties);
        }

        private void AbilityTarget(AreaAbilityProperties areaAbilityProperties)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            FoldoutField(ref isAbilityTargetFoldoutOpen, "Ability Target");

            if (!isAbilityTargetFoldoutOpen)
            {
                return;
            }

            EditorGUI.indentLevel++;
            
            ToggleField(ref areaAbilityProperties.canAffectCaster, "Can Affect Caster");
            EnumFlagsField<EntityType>(ref areaAbilityProperties.abilityTarget, "Entity Type");

            EditorGUI.indentLevel--;
        }

        private void AbilityCast(AreaAbilityProperties areaAbilityProperties)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            FoldoutField(ref isAbilityCastFoldoutOpen, "Ability Cast");
            EditorGUI.indentLevel++;

            if (isAbilityCastFoldoutOpen)
            {
                FloatField(ref areaAbilityProperties.abilityCast.timeToCast, 0, "Time To Cast");
                FloatField(ref areaAbilityProperties.abilityCast.cooldown, 0, "Cooldown");
                
                FloatField(ref areaAbilityProperties.abilityCast.minimumRangeToCast, 0, "Minimum Range To Cast");
                FloatField(ref areaAbilityProperties.abilityCast.maximumRangeToCast, 
                    areaAbilityProperties.abilityCast.minimumRangeToCast, "Maximum Range To Cast");
                
                Vector3Field(ref areaAbilityProperties.abilityCast.directionOfDetection, "Direction Of Detection (Relative To Forward)");
                
                FloatField(ref areaAbilityProperties.abilityCast.minimumAngleToCast, 0, 360, "Minimum Angle To Cast");

                if (areaAbilityProperties.abilityCast.timeToCast != 0)
                {
                    ToggleField(ref areaAbilityProperties.abilityCast.canCancelCast, "Can Cancel Cast");

                    if (areaAbilityProperties.abilityCast.canCancelCast)
                    {
                        FloatField(ref areaAbilityProperties.abilityCast.maximumAngleToCancelCast, 
                            areaAbilityProperties.abilityCast.minimumAngleToCast, 360, "Maximum Angle To Cancel Cast");
                    }   
                }
                else
                {
                    areaAbilityProperties.abilityCast.canCancelCast = false;
                }
            }
            
            AbilityAoE(ref areaAbilityProperties.abilityAoE, ref areaAbilityProperties.abilityAoEType);

            if (areaAbilityProperties.abilityAoE.duration == 0)
            {
                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
                areaAbilityProperties.abilityTrigger.hasAnEffectOnStart = true;
                AbilityEffectOnStart(ref areaAbilityProperties.abilityEffectOnStart,
                    ref areaAbilityProperties.abilityEffectOnHealthTypeOnStart,
                    ref areaAbilityProperties.doesItTriggerOnTriggerEnter, false);
                return;
            }

            AbilityTrigger(ref areaAbilityProperties.abilityTrigger, ref areaAbilityProperties.doesItTriggerOnTriggerEnter, 
                ref areaAbilityProperties.doesItTriggerOnTriggerExit, ref areaAbilityProperties.abilityEffectOnStart, 
                ref areaAbilityProperties.abilityEffectOnHealthTypeOnStart, ref areaAbilityProperties.abilityEffectOnTheDuration, 
                ref areaAbilityProperties.abilityEffectOnHealthTypeOnTheDuration, ref areaAbilityProperties.abilityEffectOnEnd, 
                ref areaAbilityProperties.abilityEffectOnHealthTypeOnEnd);
        }

        private void AbilityTrigger(ref AbilityTrigger abilityTrigger, ref bool doesItTriggerOnTriggerEnter, 
            ref bool doesItTriggerOnTriggerExit, ref AbilityEffect abilityEffectOnStart, 
            ref AbilityEffectOnHealthType abilityEffectOnHealthTypeOnStart, ref AbilityEffect abilityEffectOnTheDuration, 
            ref AbilityEffectOnHealthType abilityEffectOnHealthTypeOnTheDuration, ref AbilityEffect abilityEffectOnEnd, 
            ref AbilityEffectOnHealthType abilityEffectOnHealthTypeOnEnd)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUI.indentLevel--;
            FoldoutField(ref isAbilityTriggersFoldoutOpen, "Ability Triggers");
            EditorGUI.indentLevel++;

            if (!isAbilityTriggersFoldoutOpen)
            {
                return;
            }

            ToggleField(ref abilityTrigger.hasAnEffectOnStart, "Has An Effect On Start");
            ToggleField(ref abilityTrigger.hasAnEffectOnTheDuration, "Has An Effect On The Duration");
            ToggleField(ref abilityTrigger.hasAnEffectOnEnd, "Has An Effect On End");

            EditorGUI.indentLevel--;
            if (abilityTrigger.hasAnEffectOnStart)
            {
                EditorGUI.indentLevel++;
                EditorGUI.indentLevel++;
                AbilityEffectOnStart(ref abilityEffectOnStart, ref abilityEffectOnHealthTypeOnStart, ref doesItTriggerOnTriggerEnter, true);
                
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }

            if (abilityTrigger.hasAnEffectOnTheDuration)
            {
                EditorGUI.indentLevel++;
                EditorGUI.indentLevel++;
                AbilityEffectOnTheDuration(ref abilityEffectOnTheDuration, ref abilityEffectOnHealthTypeOnTheDuration);
                
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }

            if (!abilityTrigger.hasAnEffectOnEnd)
            {
                return;
            }

            EditorGUI.indentLevel++;
            EditorGUI.indentLevel++;
            AbilityEffectOnEnd(ref abilityEffectOnEnd, ref abilityEffectOnHealthTypeOnEnd, ref doesItTriggerOnTriggerExit);
                
            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
        }

        private void AbilityEffectOnStart(ref AbilityEffect abilityEffectOnStart, 
            ref AbilityEffectOnHealthType abilityEffectOnHealthTypeOnStart, ref bool doesTriggerOnTriggerEnter, 
            bool durationGreaterThanZero)
        {
            string message = "Ability Effect";

            if (durationGreaterThanZero)
            {
                message += " On Start";
            }
            
            FoldoutField(ref isAbilityEffectOnStartFoldoutOpen, message);

            if (!isAbilityEffectOnStartFoldoutOpen)
            {
                return;
            }

            if (!durationGreaterThanZero)
            {
                EditorGUI.indentLevel++;
            }
            
            ToggleField(ref abilityEffectOnStart.hasAnEffectOnHealth, "Has An Effect On Health");
            ToggleField(ref abilityEffectOnStart.doesSlow, "Does It Slow Down");
            ToggleField(ref abilityEffectOnStart.doesApplyAForce, "Does It Apply Force");
            

            if (durationGreaterThanZero)
            {
                ToggleField(ref doesTriggerOnTriggerEnter, "Does It Trigger On Trigger Enter");
            }
            else
            {
                doesTriggerOnTriggerEnter = false;
            }
            
            AbilityEffects(ref abilityEffectOnStart, ref abilityEffectOnHealthTypeOnStart, false);
        }

        private void AbilityEffectOnTheDuration(ref AbilityEffect abilityEffectOnTheDuration, 
            ref AbilityEffectOnHealthType abilityEffectOnHealthTypeOnTheDuration)
        {
            FoldoutField(ref isAbilityEffectOnTheDurationFoldoutOpen, "Ability Effect On The Duration");

            if (!isAbilityEffectOnTheDurationFoldoutOpen)
            {
                return;
            }
            
            ToggleField(ref abilityEffectOnTheDuration.hasAnEffectOnHealth, "Has An Effect On Health");
            ToggleField(ref abilityEffectOnTheDuration.doesSlow, "Does It Slow Down");
            ToggleField(ref abilityEffectOnTheDuration.doesApplyAForce, "Does It Apply Force");
            
            AbilityEffects(ref abilityEffectOnTheDuration, ref abilityEffectOnHealthTypeOnTheDuration, true);
        }

        private void AbilityEffectOnEnd(ref AbilityEffect abilityEffectOnEnd, 
            ref AbilityEffectOnHealthType abilityEffectOnHealthTypeOnEnd, ref bool doesTriggerOnTriggerExit)
        {
            FoldoutField(ref isAbilityEffectOnEndFoldoutOpen, "Ability Effect On End");

            if (!isAbilityEffectOnEndFoldoutOpen)
            {
                return;
            }
            
            ToggleField(ref abilityEffectOnEnd.hasAnEffectOnHealth, "Has An Effect On Health");
            ToggleField(ref abilityEffectOnEnd.doesSlow, "Does It Slow Down");
            ToggleField(ref abilityEffectOnEnd.doesApplyAForce, "Does It Apply Force");
            
            ToggleField(ref doesTriggerOnTriggerExit, "Does It Trigger On Trigger Exit");
            
            AbilityEffects(ref abilityEffectOnEnd, ref abilityEffectOnHealthTypeOnEnd, false);
        }

        private void AbilityEffects(ref AbilityEffect abilityEffect, ref AbilityEffectOnHealthType abilityEffectOnHealthType, 
            bool isOnTheDurationEffect)
        {
            if (abilityEffect.hasAnEffectOnHealth)
            {
                EditorGUI.indentLevel++;
                EffectOnHealth(ref abilityEffect, ref abilityEffectOnHealthType, isOnTheDurationEffect);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }

            if (abilityEffect.doesSlow)
            {
                EditorGUI.indentLevel++;
                SlowEffect(ref abilityEffect, isOnTheDurationEffect);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }

            if (!abilityEffect.doesApplyAForce)
            {
                return;
            }

            EditorGUI.indentLevel++;
            ForceEffect(ref abilityEffect);
            EditorGUI.indentLevel--;
        }

        private void EffectOnHealth(ref AbilityEffect abilityEffect, ref AbilityEffectOnHealthType abilityEffectOnHealthType, 
            bool isOnTheDurationEffect)
        {
            EnumField<AbilityEffectOnHealthType>(ref abilityEffectOnHealthType, "Effect On Health Type");
            
            switch (abilityEffectOnHealthType)
            {
                case AbilityEffectOnHealthType.DAMAGE:
                    if (isOnTheDurationEffect)
                    {
                        UintField(ref abilityEffect.healthModificationValue, 0, "Damage per Tick");
                        abilityEffect.effectOnHealthDuration = 0;
                        break;
                    }
                    
                    ToggleField(ref abilityEffect.isEffectOnHealthAttachedToEntity, "Does It Remain Attached To The Entity");

                    if (!abilityEffect.isEffectOnHealthAttachedToEntity)
                    {
                        UintField(ref abilityEffect.healthModificationValue, 0, "Damage");
                        break;
                    }
                    
                    UintField(ref abilityEffect.healthModificationValue, 0, "Damage per Tick");
                    FloatField(ref abilityEffect.effectOnHealthDuration, 0, "Damage Duration");
                    break;
                
                case AbilityEffectOnHealthType.HEAL:
                    if (isOnTheDurationEffect)
                    {
                        UintField(ref abilityEffect.healthModificationValue, 0, "Heal per Tick");
                        abilityEffect.effectOnHealthDuration = 0;
                        break;
                    }
                    
                    ToggleField(ref abilityEffect.isEffectOnHealthAttachedToEntity, "Does It Remain Attached To The Entity");
                    
                    if (!abilityEffect.isEffectOnHealthAttachedToEntity)
                    {
                        UintField(ref abilityEffect.healthModificationValue, 0, "Heal");
                        break;
                    }
                    
                    UintField(ref abilityEffect.healthModificationValue, 0, "Heal per Tick");
                    FloatField(ref abilityEffect.effectOnHealthDuration, 0, "Heal Duration");
                    break;
            }
        }

        private void SlowEffect(ref AbilityEffect abilityEffect, bool isOnTheDurationEffect)
        {
            if (!isOnTheDurationEffect)
            {
                ToggleField(ref abilityEffect.doesDecreaseOverTime, "Does It Decrease Over Time");
            }
            
            UintField(ref abilityEffect.slowPercent, 0, 100, "Slow Percent");
                    
            if (isOnTheDurationEffect)
            {
                abilityEffect.isSlowAttachedToEntity = false;
                return;
            }
            
            FloatField(ref abilityEffect.slowDuration, 0, "Slow Duration");
            abilityEffect.isSlowAttachedToEntity = true;
        }

        private void ForceEffect(ref AbilityEffect abilityEffect)
        {
            Vector3Field(ref abilityEffect.forceDirection, "Force Direction");
            FloatField(ref abilityEffect.forceStrength, 0, "Force Strength");
            
            ToggleField(ref abilityEffect.doesForceComesFromCenterOfTheArea, "Does The Force Come From The Center Of The Area");
        }

        private void AbilityAoE(ref AbilityAoE abilityAoE, ref AbilityAoEType abilityAoEType)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            EditorGUI.indentLevel--;
            FoldoutField(ref isAbilityAoEFoldoutOpen, "Ability AoE");
            EditorGUI.indentLevel++;

            if (!isAbilityAoEFoldoutOpen)
            {
                return;
            }
            
            ToggleField(ref abilityAoE.isAttachedToCaster, "Is Attached To Caster");
            
            Vector3Field(ref abilityAoE.relativePositionToCaster, "Relative Position To Caster");
            
            FloatField(ref abilityAoE.duration, 0, "Duration");
            
            EnumField<AbilityAoEType>(ref abilityAoEType, "Ability AoE Type");
            
            switch (abilityAoEType)
            {
                case AbilityAoEType.RECTANGULAR:
                    Vector3Field(ref abilityAoE.direction, "Direction");
                    FloatField(ref abilityAoE.height, 0, "Height");
                    FloatField(ref abilityAoE.width, 0, "Width");
                    FloatField(ref abilityAoE.length, 0, "Length");

                    if (abilityAoE.duration == 0)
                    {
                        abilityAoE.doesHeightChangeOverTheTime = false;
                        abilityAoE.doesWidthChangeOverTheTime = false;
                        abilityAoE.doesLengthChangeOverTheTime = false;
                        return;
                    }
                    
                    ToggleField(ref abilityAoE.doesHeightChangeOverTheTime, "Does Height Change Over Time");
                    ToggleField(ref abilityAoE.doesWidthChangeOverTheTime, "Does Width Change Over Time");
                    ToggleField(ref abilityAoE.doesLengthChangeOverTheTime, "Does Length Change Over Time");
                    
                    if (abilityAoE.doesHeightChangeOverTheTime)
                    {
                        CurveField(ref abilityAoE.heightChangeOverTime, abilityAoE.height, abilityAoE.duration, "Height Over Time");
                    }
                    
                    if (abilityAoE.doesWidthChangeOverTheTime)
                    {
                        CurveField(ref abilityAoE.widthChangeOverTime, abilityAoE.width, abilityAoE.duration, "Width Over Time");
                    }
                    
                    if (abilityAoE.doesLengthChangeOverTheTime)
                    {
                        CurveField(ref abilityAoE.lengthChangeOverTime, abilityAoE.length, abilityAoE.duration, "Length Over Time");
                    }
                    
                    return;
                
                case AbilityAoEType.SPHERICAL:
                    Vector3Field(ref abilityAoE.direction, "Direction");
                    
                    FloatField(ref abilityAoE.radius, 0, "Radius");

                    if (abilityAoE.duration == 0)
                    {
                        abilityAoE.doesRadiusChangeOverTheTime = false;
                        return;
                    }
                    
                    ToggleField(ref abilityAoE.doesRadiusChangeOverTheTime, "Does Radius Change Over Time");
                    
                    if (abilityAoE.doesRadiusChangeOverTheTime)
                    {
                        CurveField(ref abilityAoE.radiusChangeOverTime, abilityAoE.radius, abilityAoE.duration, "Radius Over Time");
                    }
                    return;
                
                /*case AbilityAoEType.CONICAL:
                    Vector3Field(ref abilityAoE.direction, "Direction");
                    FloatField(ref abilityAoE.height, 0, "Height");
                    FloatField(ref abilityAoE.radius, 0, "Radius");

                    if (abilityAoE.duration == 0)
                    {
                        return;
                    }
                    
                    ToggleField(ref abilityAoE.doesHeightChangeOverTheTime, "Does Height Change Over Time");
                    ToggleField(ref abilityAoE.doesRadiusChangeOverTheTime, "Does Radius Change Over Time");
                    
                    if (abilityAoE.doesHeightChangeOverTheTime)
                    {
                        CurveField(ref abilityAoE.heightChangeOverTime, abilityAoE.duration, "Height's Multiplier Over Time");
                    }
                    
                    if (abilityAoE.doesRadiusChangeOverTheTime)
                    {
                        CurveField(ref abilityAoE.radiusChangeOverTime, abilityAoE.duration, "Radius' Multiplier Over Time");
                    }
                    break;*/
                
                case AbilityAoEType.CUSTOM_MESH:
                    ObjectField(ref abilityAoE.customMeshPrefab, "Custom Mesh Prefab");

                    if (abilityAoE.customMeshPrefab == null)
                    {
                        return;
                    }

                    if (abilityAoE.duration == 0)
                    {
                        abilityAoE.doesScaleChangeOverTheTime = false;
                        return;
                    }
                    
                    Vector3 scales = abilityAoE.customMeshPrefab.transform.lossyScale;
                    bool areScalesEquals = scales.x == scales.y && scales.y == scales.z;
                    
                    if (areScalesEquals)
                    {
                        ToggleField(ref abilityAoE.doesScaleChangeOverTheTime, "Does Scale Change Over Time");

                        if (!abilityAoE.doesScaleChangeOverTheTime)
                        {
                            return;
                        }
                        
                        ToggleField(ref unifyScales, "Unify Scales");

                        if (unifyScales)
                        {
                            CurveField(ref abilityAoE.scaleChangeOverTime, scales.x, abilityAoE.duration, "Scale Over Time");
                            return;
                        }
                    }
                        
                    ToggleField(ref abilityAoE.doesXScaleChangeOverTheTime, "Does X Scale Change Over Time");
                    ToggleField(ref abilityAoE.doesYScaleChangeOverTheTime, "Does Y Scale Change Over Time");
                    ToggleField(ref abilityAoE.doesZScaleChangeOverTheTime, "Does Z Scale Change Over Time");

                    if (abilityAoE.doesXScaleChangeOverTheTime)
                    {
                        CurveField(ref abilityAoE.XScaleChangeOverTime, scales.x, abilityAoE.duration, "X Scale Over Time");
                    }

                    if (abilityAoE.doesYScaleChangeOverTheTime)
                    {
                        CurveField(ref abilityAoE.YScaleChangeOverTime, scales.y, abilityAoE.duration, "Y Scale Over Time");
                    }

                    if (!abilityAoE.doesZScaleChangeOverTheTime)
                    {
                        return;
                    }
                    
                    CurveField(ref abilityAoE.ZScaleChangeOverTime, scales.z, abilityAoE.duration, "Z Scale Over Time");
                    return;
            }
        }
    }
}