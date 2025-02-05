using System;
using AI.Combat.AbilitySpecs;
using AI.Combat.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Editor.Enemies
{
    [CustomEditor(typeof(AgentAbility))]
    public class AgentAbilityEditor : MyEditor
    {
        private bool isAbilityCastFoldoutOpen = true;
        private bool isAbilityProjectileFoldoutOpen = true;
        private bool isAbilityTriggersFoldoutOpen = true;
        private bool isAbilityEffectOnStartFoldoutOpen = true;
        private bool isAbilityEffectOnTheDurationFoldoutOpen = true;
        private bool isAbilityEffectOnEndFoldoutOpen = true;
        private bool isAbilityAoEFoldoutOpen = true;

        public override void OnInspectorGUI()
        {
            InitializeStyles();
            
            AgentAbility agentAbility = (AgentAbility)target;
            
            AbilityTarget(ref agentAbility.abilityTarget);

            AbilityCast(agentAbility);

            if (!GUI.changed)
            {
                return;
            }

            EditorUtility.SetDirty(agentAbility);
        }

        private void AbilityTarget(ref AbilityTarget abilityTarget)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            LabelField("Ability Target", 150);
            EnumField<AbilityTarget>(ref abilityTarget, 0.563f);
            EditorGUILayout.EndHorizontal();
        }

        private void AbilityCast(AgentAbility agentAbility)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            FoldoutField(ref isAbilityCastFoldoutOpen, "Ability Cast");
            EditorGUI.indentLevel++;

            if (isAbilityCastFoldoutOpen)
            {
                FloatField(ref agentAbility.abilityCast.minimumRangeToCast, 0, "Minimum Range To Cast");
                FloatField(ref agentAbility.abilityCast.maximumRangeToCast, 0, "Minimum Range To Cast");
                FloatField(ref agentAbility.abilityCast.timeToCast, 0, "Time To Cast");
                FloatField(ref agentAbility.abilityCast.cooldown, 0, "Cooldown");
                EnumField<AbilityCastType>(ref agentAbility.abilityCastType, "Cast Type");
            }

            switch (agentAbility.abilityCastType)
            {
                case AbilityCastType.OMNIPRESENT:
                    FloatField(ref agentAbility.abilityCast.duration, "Duration");

                    if (agentAbility.abilityCast.duration == 0)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUI.indentLevel--;
                        agentAbility.abilityTrigger.doesEffectOnStart = true;
                        AbilityEffectOnStart(agentAbility.abilityEffectOnStart,
                            ref agentAbility.abilityEffectOnHealthTypeOnStart, false,
                            ref agentAbility.doesItTriggerOnTriggerEnter, false);
                        break;
                    }

                    AbilityTrigger(agentAbility.abilityTrigger, ref agentAbility.doesItTriggerOnTriggerEnter, 
                        ref agentAbility.doesItTriggerOnTriggerExit, agentAbility.abilityCastType == AbilityCastType.OMNIPRESENT, 
                        agentAbility.abilityEffectOnStart, ref agentAbility.abilityEffectOnHealthTypeOnStart, 
                        agentAbility.abilityEffectOnTheDuration, ref agentAbility.abilityEffectOnHealthTypeOnTheDuration, 
                        agentAbility.abilityEffectOnEnd, ref agentAbility.abilityEffectOnHealthTypeOnEnd);
                    
                    break;
                
                case AbilityCastType.NO_PROJECTILE:
                    if (isAbilityCastFoldoutOpen)
                    {
                        ToggleField(ref agentAbility.abilityCast.doesSpawnInsideCaster, "Does Relative Position To Caster Change");

                        if (!agentAbility.abilityCast.doesSpawnInsideCaster)
                        {
                            agentAbility.abilityCast.relativeSpawnPosition = Vector3.zero;
                        }
                        else
                        {
                            Vector3Field(ref agentAbility.abilityCast.relativeSpawnPosition, "Relative Position To Caster");
                        }
                    
                        ToggleField(ref agentAbility.abilityCast.isAttachedToCaster, "Is Attached To Caster");
                    }
                    
                    AbilityAoE(agentAbility.abilityAoE, ref agentAbility.abilityAoEType);

                    if (agentAbility.abilityAoE.duration == 0)
                    {
                        EditorGUILayout.Space();
                        EditorGUI.indentLevel--;
                        agentAbility.abilityTrigger.doesEffectOnStart = true;
                        AbilityEffectOnStart(agentAbility.abilityEffectOnStart,
                            ref agentAbility.abilityEffectOnHealthTypeOnStart, false,
                            ref agentAbility.doesItTriggerOnTriggerEnter, false);
                        break;
                    }

                    AbilityTrigger(agentAbility.abilityTrigger, ref agentAbility.doesItTriggerOnTriggerEnter, 
                        ref agentAbility.doesItTriggerOnTriggerExit, agentAbility.abilityCastType == AbilityCastType.OMNIPRESENT, 
                        agentAbility.abilityEffectOnStart, ref agentAbility.abilityEffectOnHealthTypeOnStart, 
                        agentAbility.abilityEffectOnTheDuration, ref agentAbility.abilityEffectOnHealthTypeOnTheDuration, 
                        agentAbility.abilityEffectOnEnd, ref agentAbility.abilityEffectOnHealthTypeOnEnd);
                    break;
                
                case AbilityCastType.PARABOLA_PROJECTILE:
                case AbilityCastType.STRAIGHT_LINE_PROJECTILE:
                    if (isAbilityCastFoldoutOpen)
                    {
                        if (!agentAbility.abilityCast.doesSpawnInsideCaster)
                        {
                            agentAbility.abilityCast.relativeSpawnPosition = Vector3.zero;
                        }
                        else
                        {
                            Vector3Field(ref agentAbility.abilityCast.relativeSpawnPosition, "Relative Position To Caster");
                        }
                    }

                    agentAbility.abilityCast.isAttachedToCaster = false;
                    
                    AbilityProjectile(agentAbility.abilityProjectile);
                    
                    AbilityAoE(agentAbility.abilityAoE, ref agentAbility.abilityAoEType);

                    if (agentAbility.abilityAoE.duration == 0)
                    {
                        EditorGUILayout.Space();
                        EditorGUI.indentLevel--;
                        agentAbility.abilityTrigger.doesEffectOnStart = true;
                        AbilityEffectOnStart(agentAbility.abilityEffectOnStart,
                            ref agentAbility.abilityEffectOnHealthTypeOnStart, false,
                            ref agentAbility.doesItTriggerOnTriggerEnter, false);
                        break;
                    }

                    AbilityTrigger(agentAbility.abilityTrigger, ref agentAbility.doesItTriggerOnTriggerEnter, 
                        ref agentAbility.doesItTriggerOnTriggerExit, agentAbility.abilityCastType == AbilityCastType.OMNIPRESENT, 
                        agentAbility.abilityEffectOnStart, ref agentAbility.abilityEffectOnHealthTypeOnStart, 
                        agentAbility.abilityEffectOnTheDuration, ref agentAbility.abilityEffectOnHealthTypeOnTheDuration, 
                        agentAbility.abilityEffectOnEnd, ref agentAbility.abilityEffectOnHealthTypeOnEnd);
                    break;
            }
        }

        private void AbilityTrigger(AbilityTrigger abilityTrigger, ref bool doesItTriggerOnTriggerEnter, 
            ref bool doesItTriggerOnTriggerExit, bool isCastTypeOmnipresent, 
            AbilityEffect abilityEffectOnStart, ref AbilityEffectOnHealthType abilityEffectOnHealthTypeOnStart, 
            AbilityEffect abilityEffectOnTheDuration, ref AbilityEffectOnHealthType abilityEffectOnHealthTypeOnTheDuration, 
            AbilityEffect abilityEffectOnEnd, ref AbilityEffectOnHealthType abilityEffectOnHealthTypeOnEnd)
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

            ToggleField(ref abilityTrigger.doesEffectOnStart, "Does Effect On Start");
            ToggleField(ref abilityTrigger.doesEffectOnTheDuration, "Does Effect On The Duration");
            ToggleField(ref abilityTrigger.doesEffectOnEnd, "Does Effect On End");

            if (abilityTrigger.doesEffectOnStart)
            {
                EditorGUI.indentLevel++;
                AbilityEffectOnStart(abilityEffectOnStart, ref abilityEffectOnHealthTypeOnStart, 
                    !isCastTypeOmnipresent, ref doesItTriggerOnTriggerEnter, true);
                
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }

            if (abilityTrigger.doesEffectOnTheDuration)
            {
                EditorGUI.indentLevel++;
                EditorGUI.indentLevel++;
                AbilityEffectOnTheDuration(abilityEffectOnTheDuration, ref abilityEffectOnHealthTypeOnTheDuration);
                
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }

            if (!abilityTrigger.doesEffectOnEnd)
            {
                return;
            }

            EditorGUI.indentLevel++;
            EditorGUI.indentLevel++;
            AbilityEffectOnEnd(abilityEffectOnEnd, ref abilityEffectOnHealthTypeOnEnd, !isCastTypeOmnipresent, 
                ref doesItTriggerOnTriggerExit);
                
            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
        }

        private void AbilityEffectOnStart(AbilityEffect abilityEffectOnStart, 
            ref AbilityEffectOnHealthType abilityEffectOnHealthTypeOnStart, bool canShowTrigger, 
            ref bool doesTriggerOnTriggerEnter, bool durationGreaterThanZero)
        {
            string message = "Ability Effect";

            if (durationGreaterThanZero)
            {
                message += " On Start";
            }
            
            FoldoutField(ref isAbilityEffectOnStartFoldoutOpen, message);
            if (!canShowTrigger)
            {
                EditorGUI.indentLevel++;
            }

            if (!isAbilityEffectOnStartFoldoutOpen)
            {
                return;
            }
            
            ToggleField(ref abilityEffectOnStart.hasAnEffectOnHealth, "Has Effect On Health");
            ToggleField(ref abilityEffectOnStart.doesSlow, "Does Slow");
            ToggleField(ref abilityEffectOnStart.doesApplyAForce, "Does Apply Force");
            
            if (canShowTrigger)
            {
                ToggleField(ref doesTriggerOnTriggerEnter, "Does Trigger On Trigger Enter");
            }
            else
            {
                doesTriggerOnTriggerEnter = false;
            }
            
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
            
            ToggleField(ref abilityEffectOnTheDuration.hasAnEffectOnHealth, "Has Effect On Health");
            ToggleField(ref abilityEffectOnTheDuration.doesSlow, "Does Slow");
            ToggleField(ref abilityEffectOnTheDuration.doesApplyAForce, "Does Apply Force");
            
            AbilityEffects(abilityEffectOnTheDuration, ref abilityEffectOnHealthTypeOnTheDuration, true);
        }

        private void AbilityEffectOnEnd(AbilityEffect abilityEffectOnEnd, 
            ref AbilityEffectOnHealthType abilityEffectOnHealthTypeOnEnd, bool canShowTrigger,
            ref bool doesTriggerOnTriggerExit)
        {
            FoldoutField(ref isAbilityEffectOnEndFoldoutOpen, "Ability Effect On End");

            if (!isAbilityEffectOnEndFoldoutOpen)
            {
                return;
            }
            
            ToggleField(ref abilityEffectOnEnd.hasAnEffectOnHealth, "Has Effect On Health");
            ToggleField(ref abilityEffectOnEnd.doesSlow, "Does Slow");
            ToggleField(ref abilityEffectOnEnd.doesApplyAForce, "Does Apply Force");
            
            if (canShowTrigger)
            {
                ToggleField(ref doesTriggerOnTriggerExit, "Does Trigger On Trigger Exit");
            }
            
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
                    
                    ToggleField(ref abilityEffect.isEffectOnHealthAttachedToEntity, "Does Remain Attached To Entity");

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
                    
                    ToggleField(ref abilityEffect.isEffectOnHealthAttachedToEntity, "Does Remain Attached To Entity");
                    
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
                ToggleField(ref abilityEffect.doesDecreaseOverTime, "Does Decrease Over Time");
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
            
            ToggleField(ref abilityEffect.doesForceComesFromCenterOfTheArea, "Does Force Comes From The Center Of The Area");
        }

        private void AbilityProjectile(AbilityProjectile abilityProjectile)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            EditorGUI.indentLevel--;
            FoldoutField(ref isAbilityProjectileFoldoutOpen, "Ability Projectile");
            EditorGUI.indentLevel++;

            if (!isAbilityProjectileFoldoutOpen)
            {
                return;
            }

            ObjectField<GameObject>(ref abilityProjectile.projectilePrefab, "Projectile Prefab");
            FloatField(ref abilityProjectile.projectileSpeed, 0, "Projectile Speed");
            ToggleField(ref abilityProjectile.doesVanishOnImpact, "Does Vanish On Impact");
            ToggleField(ref abilityProjectile.doesVanishOverTime, "Does Vanish Over Time");

            if (!abilityProjectile.doesVanishOverTime)
            {
                return;
            }

            FloatField(ref abilityProjectile.timeToVanish, "Time To Vanish");
            ToggleField(ref abilityProjectile.doesExplodeOnVanishOverTime, "Does Explode On Vanish Over Time");
        }

        private void AbilityAoE(AbilityAoE abilityAoE, ref AbilityAoEType abilityAoEType)
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
                    
                    break;
                
                case AbilityAoEType.SPHERICAL:
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
                    break;
                
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

                    if (abilityAoE.duration == 0)
                    {
                        abilityAoE.doesScaleChangeOverTheTime = false;
                        return;
                    }
                    
                    ToggleField(ref abilityAoE.doesScaleChangeOverTheTime, "Does Scale Change Over Time");

                    if (abilityAoE.doesScaleChangeOverTheTime)
                    {
                        CurveField(ref abilityAoE.scaleChangeOverTime, 1, abilityAoE.duration, "Scale Over Time");
                    }
                    break;
            }
        }
    }
}