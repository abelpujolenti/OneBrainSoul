using AI.Combat.AbilitySpecs;
using AI.Combat.ScriptableObjects;
using ECS.Entities;
using UnityEditor;
using UnityEngine;

namespace Editor.Enemies
{
    [CustomEditor(typeof(BasicAbilityProperties))]
    public class BasicAbilityEditor : MyEditor
    {
        private bool isAbilityTargetFoldoutOpen = true;
        private bool isAbilityCastFoldoutOpen = true;
        private bool isAbilityTriggersFoldoutOpen = true;
        private bool isAbilityEffectOnStartFoldoutOpen = true;
        private bool isAbilityEffectOnTheDurationFoldoutOpen = true;
        private bool isAbilityEffectOnEndFoldoutOpen = true;

        public override void OnInspectorGUI()
        {
            InitializeStyles();
            
            BasicAbilityProperties basicAbilityProperties = (BasicAbilityProperties)target;
            
            AbilityTarget(basicAbilityProperties);

            AbilityCast(basicAbilityProperties);

            if (!GUI.changed)
            {
                return;
            }

            EditorUtility.SetDirty(basicAbilityProperties);
        }

        private void AbilityTarget(BasicAbilityProperties basicAbilityProperties)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            FoldoutField(ref isAbilityTargetFoldoutOpen, "Ability Target");

            if (!isAbilityTargetFoldoutOpen)
            {
                return;
            }

            EditorGUI.indentLevel++;
            
            ToggleField(ref basicAbilityProperties.canAffectCaster, "Can Affect Caster");
            EnumFlagsField<EntityType>(ref basicAbilityProperties.abilityTarget, "Entity Type");

            EditorGUI.indentLevel--;
        }

        private void AbilityCast(BasicAbilityProperties basicAbilityProperties)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            FoldoutField(ref isAbilityCastFoldoutOpen, "Ability Cast");
            EditorGUI.indentLevel++;

            if (isAbilityCastFoldoutOpen)
            {
                FloatField(ref basicAbilityProperties.abilityCast.timeToCast, 0, "Time To Cast");
                FloatField(ref basicAbilityProperties.abilityCast.cooldown, 0, "Cooldown");
                FloatField(ref basicAbilityProperties.abilityCast.duration, "Duration");
                
                FloatField(ref basicAbilityProperties.abilityCast.minimumRangeToCast, 0, "Minimum Range To Cast");
                FloatField(ref basicAbilityProperties.abilityCast.maximumRangeToCast, 
                    basicAbilityProperties.abilityCast.minimumRangeToCast, "Maximum Range To Cast");
                
                Vector3Field(ref basicAbilityProperties.abilityCast.directionOfDetection, "Direction Of Detection (Relative To Head's Forward)");
                
                FloatField(ref basicAbilityProperties.abilityCast.minimumAngleToCast, 0, 360, "Minimum Angle To Cast");

                if (basicAbilityProperties.abilityCast.timeToCast != 0)
                {
                    ToggleField(ref basicAbilityProperties.abilityCast.canCancelCast, "Can Cancel Cast");

                    if (basicAbilityProperties.abilityCast.canCancelCast)
                    {
                        FloatField(ref basicAbilityProperties.abilityCast.maximumAngleToCancelCast, 
                            basicAbilityProperties.abilityCast.minimumAngleToCast, 360, "Maximum Angle To Cancel Cast");
                    }   
                }
                else
                {
                    basicAbilityProperties.abilityCast.canCancelCast = false;
                }
            }

            if (basicAbilityProperties.abilityCast.duration == 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
                basicAbilityProperties.abilityTrigger.hasAnEffectOnStart = true;
                AbilityEffectOnStart(basicAbilityProperties.abilityEffectOnStart,
                    ref basicAbilityProperties.abilityEffectOnHealthTypeOnStart, false);
                return;
            }

            AbilityTrigger(basicAbilityProperties.abilityTrigger, basicAbilityProperties.abilityEffectOnStart, 
                ref basicAbilityProperties.abilityEffectOnHealthTypeOnStart, basicAbilityProperties.abilityEffectOnTheDuration, 
                ref basicAbilityProperties.abilityEffectOnHealthTypeOnTheDuration, basicAbilityProperties.abilityEffectOnEnd, 
                ref basicAbilityProperties.abilityEffectOnHealthTypeOnEnd);
        }

        private void AbilityTrigger(AbilityTrigger abilityTrigger, AbilityEffect abilityEffectOnStart, 
            ref AbilityEffectOnHealthType abilityEffectOnHealthTypeOnStart, AbilityEffect abilityEffectOnTheDuration, 
            ref AbilityEffectOnHealthType abilityEffectOnHealthTypeOnTheDuration, AbilityEffect abilityEffectOnEnd, 
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

            if (abilityTrigger.hasAnEffectOnStart)
            {
                EditorGUI.indentLevel++;
                AbilityEffectOnStart(abilityEffectOnStart, ref abilityEffectOnHealthTypeOnStart, true);
                
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }

            if (abilityTrigger.hasAnEffectOnTheDuration)
            {
                EditorGUI.indentLevel++;
                EditorGUI.indentLevel++;
                AbilityEffectOnTheDuration(abilityEffectOnTheDuration, ref abilityEffectOnHealthTypeOnTheDuration);
                
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
            AbilityEffectOnEnd(abilityEffectOnEnd, ref abilityEffectOnHealthTypeOnEnd);
                
            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
        }

        private void AbilityEffectOnStart(AbilityEffect abilityEffectOnStart, 
            ref AbilityEffectOnHealthType abilityEffectOnHealthTypeOnStart, bool durationGreaterThanZero)
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
            
            AbilityEffects(abilityEffectOnStart, ref abilityEffectOnHealthTypeOnStart, false);
        }

        private void AbilityEffectOnTheDuration(AbilityEffect abilityEffectOnTheDuration, 
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
            
            AbilityEffects(abilityEffectOnTheDuration, ref abilityEffectOnHealthTypeOnTheDuration, true);
        }

        private void AbilityEffectOnEnd(AbilityEffect abilityEffectOnEnd, 
            ref AbilityEffectOnHealthType abilityEffectOnHealthTypeOnEnd)
        {
            FoldoutField(ref isAbilityEffectOnEndFoldoutOpen, "Ability Effect On End");

            if (!isAbilityEffectOnEndFoldoutOpen)
            {
                return;
            }
            
            ToggleField(ref abilityEffectOnEnd.hasAnEffectOnHealth, "Has An Effect On Health");
            ToggleField(ref abilityEffectOnEnd.doesSlow, "Does It Slow Down");
            ToggleField(ref abilityEffectOnEnd.doesApplyAForce, "Does It Apply Force");
            
            AbilityEffects(abilityEffectOnEnd, ref abilityEffectOnHealthTypeOnEnd, false);
        }

        private void AbilityEffects(AbilityEffect abilityEffect, ref AbilityEffectOnHealthType abilityEffectOnHealthType, 
            bool isOnTheDurationEffect)
        {
            if (abilityEffect.hasAnEffectOnHealth)
            {
                EditorGUI.indentLevel++;
                EffectOnHealth(abilityEffect, ref abilityEffectOnHealthType, isOnTheDurationEffect);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }

            if (abilityEffect.doesSlow)
            {
                EditorGUI.indentLevel++;
                SlowEffect(abilityEffect, isOnTheDurationEffect);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }

            if (!abilityEffect.doesApplyAForce)
            {
                return;
            }

            EditorGUI.indentLevel++;
            ForceEffect(abilityEffect);
            EditorGUI.indentLevel--;
        }

        private void EffectOnHealth(AbilityEffect abilityEffect, ref AbilityEffectOnHealthType abilityEffectOnHealthType, 
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

        private void SlowEffect(AbilityEffect abilityEffect, bool isOnTheDurationEffect)
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

        private void ForceEffect(AbilityEffect abilityEffect)
        {
            Vector3Field(ref abilityEffect.forceDirection, "Force Direction");
            FloatField(ref abilityEffect.forceStrength, 0, "Force Strength");
            
            ToggleField(ref abilityEffect.doesForceComesFromCenterOfTheArea, "Does The Force Come From The Center Of The Area");
        }
    }
}