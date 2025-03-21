﻿using AI.Combat.AbilitySpecs;
using AI.Combat.ScriptableObjects;
using ECS.Entities;
using UnityEditor;
using UnityEngine;

namespace Editor.Enemies
{
    [CustomEditor(typeof(ProjectileAbilityProperties))]
    public class ProjectileAbilityEditor : MyEditor
    {
        private bool isAbilityTargetFoldoutOpen = true;
        private bool isAbilityCastFoldoutOpen = true;
        private bool isAbilityProjectileFoldoutOpen = true;
        private bool isAbilityTriggersFoldoutOpen = true;
        private bool isAbilityEffectOnStartFoldoutOpen = true;
        private bool isAbilityEffectOnTheDurationFoldoutOpen = true;
        private bool isAbilityEffectOnEndFoldoutOpen = true;
        private bool isAbilityAoEFoldoutOpen = true;
        private bool isAbilityMovementFoldoutOpen = true;

        private SerializedProperty executeAbilitySound;
        private SerializedProperty abilityAoESound;
        private SerializedProperty projectileSound;

        private void OnEnable()
        {
            executeAbilitySound = serializedObject.FindProperty("executeAbilitySound");
            abilityAoESound = serializedObject.FindProperty("abilityAoESound");
            projectileSound = serializedObject.FindProperty("projectileSound");
        }

        public override void OnInspectorGUI()
        {
            InitializeStyles();
            
            ProjectileAbilityProperties projectileAbilityProperties = (ProjectileAbilityProperties)target;
            
            AbilityTarget(projectileAbilityProperties);
            
            AbilitySound();

            AbilityCast(projectileAbilityProperties);

            if (!GUI.changed)
            {
                return;
            }

            EditorUtility.SetDirty(projectileAbilityProperties);
        }

        private void AbilityTarget(ProjectileAbilityProperties projectileAbilityProperties)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            FoldoutField(ref isAbilityTargetFoldoutOpen, "Ability Target");

            if (!isAbilityTargetFoldoutOpen)
            {
                return;
            }

            EditorGUI.indentLevel++;
            
            EnumFlagsField<EntityType>(ref projectileAbilityProperties.abilityTarget, "Entity Type");
            
            EditorGUILayout.Space();

            projectileAbilityProperties.typesAffectedByTheAbility =
                (projectileAbilityProperties.typesAffectedByTheAbility & ~projectileAbilityProperties.abilityTarget) |
                projectileAbilityProperties.abilityTarget;
            
            ToggleField(ref projectileAbilityProperties.canAffectCaster, "Can Affect Caster");
            EnumFlagsField<EntityType>(ref projectileAbilityProperties.typesAffectedByTheAbility, "Types Affected By The Ability");

            EditorGUI.indentLevel--;
        }

        private void AbilitySound()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(executeAbilitySound, new GUIContent("Execute Ability Sound"));
            serializedObject.ApplyModifiedProperties();
        }

        private void AbilityCast(ProjectileAbilityProperties projectileAbilityProperties)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            FoldoutField(ref isAbilityCastFoldoutOpen, "Ability Cast");
            EditorGUI.indentLevel++;

            if (isAbilityCastFoldoutOpen)
            {
                FloatField(ref projectileAbilityProperties.abilityCast.timeToCast, 0, "Time To Cast");
                FloatField(ref projectileAbilityProperties.abilityCast.cooldown, 0, "Cooldown");

                if (projectileAbilityProperties.abilityCast.timeToCast != 0)
                {
                    ToggleField(ref projectileAbilityProperties.abilityCast.canCancelCast, "Can Cancel Cast");
                }
                else
                {
                    projectileAbilityProperties.abilityCast.canCancelCast = false;
                }
            }
            
            AbilityProjectile(ref projectileAbilityProperties.abilityProjectile);
            
            AbilityAoE(ref projectileAbilityProperties.abilityAoE, ref projectileAbilityProperties.abilityAoEType);
            
            AbilityMovement(ref projectileAbilityProperties.abilityMovement, projectileAbilityProperties.abilityAoE.duration);

            if (projectileAbilityProperties.abilityAoE.duration == 0)
            {
                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
                projectileAbilityProperties.abilityTrigger.hasAnEffectOnStart = true;
                AbilityEffectOnStart(ref projectileAbilityProperties.abilityEffectOnStart,
                    ref projectileAbilityProperties.abilityEffectOnHealthTypeOnStart,
                    ref projectileAbilityProperties.doesItTriggerOnTriggerEnter, false);
                return;
            }

            AbilityTrigger(ref projectileAbilityProperties.abilityTrigger, ref projectileAbilityProperties.doesItTriggerOnTriggerEnter, 
                ref projectileAbilityProperties.doesItTriggerOnTriggerExit, ref projectileAbilityProperties.abilityEffectOnStart, 
                ref projectileAbilityProperties.abilityEffectOnHealthTypeOnStart, ref projectileAbilityProperties.abilityEffectOnTheDuration, 
                ref projectileAbilityProperties.abilityEffectOnHealthTypeOnTheDuration, ref projectileAbilityProperties.abilityEffectOnEnd, 
                ref projectileAbilityProperties.abilityEffectOnHealthTypeOnEnd);
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
                AbilityEffectOnStart(ref abilityEffectOnStart, ref abilityEffectOnHealthTypeOnStart, 
                    ref doesItTriggerOnTriggerEnter, true);
                
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
            ref AbilityEffectOnHealthType abilityEffectOnHealthTypeOnStart, 
            ref bool doesTriggerOnTriggerEnter, bool durationGreaterThanZero)
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
            
            EnumField(ref abilityEffect.abilityEffectForceType, "Ability Effect Force Type");
        }

        private void AbilityProjectile(ref AbilityProjectile abilityProjectile)
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
            
            serializedObject.Update();
            EditorGUILayout.PropertyField(projectileSound, new GUIContent("Projectile Sound"));
            serializedObject.ApplyModifiedProperties();
            
            Vector3Field(ref abilityProjectile.relativePositionToCaster, "Relative Position To Caster");
            ObjectField<GameObject>(ref abilityProjectile.projectilePrefab, "Projectile Prefab");
            ObjectField(ref abilityProjectile.objectWithParticleSystem, "Object With Trail Particle System");
            Vector3Field(ref abilityProjectile.relativePositionForParticles, "Relative Position To Projectile");
            //ToggleField(ref abilityProjectile.makesParabola, "Makes a Parabola");
            FloatField(ref abilityProjectile.projectileSpeed, 0, "Projectile Speed");
            UintField(ref abilityProjectile.instances, 1, "Projectile Instances");
            FloatField(ref abilityProjectile.dispersionRatePer1Meter, 0, "Dispersion Rate Per 1 Meter");
            /*ToggleField(ref abilityProjectile.doesVanishOnImpact, "Does It Vanish On Impact");
            ToggleField(ref abilityProjectile.doesVanishOverTime, "Does It Vanish Over Time");

            if (!abilityProjectile.doesVanishOverTime)
            {
                abilityProjectile.doesVanishOnImpact = true;
                return;
            }

            FloatField(ref abilityProjectile.timeToVanish, "Time To Vanish");
            ToggleField(ref abilityProjectile.doesExplodeOnVanishOverTime, "Does It Explode On Vanish Due To Time");*/
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

            abilityAoE.isAttachedToCaster = false;

            serializedObject.Update();
            EditorGUILayout.PropertyField(abilityAoESound, new GUIContent("Ability AoE Sound"));
            serializedObject.ApplyModifiedProperties();

            Vector3Field(ref abilityAoE.relativePositionToCaster, "Relative Position To Projectile");
            
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
                        break;
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
                    Vector3Field(ref abilityAoE.direction, "Direction");
                    
                    FloatField(ref abilityAoE.radius, 0, "Radius");

                    if (abilityAoE.duration == 0)
                    {
                        abilityAoE.doesRadiusChangeOverTheTime = false;
                        break;
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
                        break;
                    }
                    
                    ToggleField(ref abilityAoE.doesScaleChangeOverTheTime, "Does Scale Change Over Time");

                    if (abilityAoE.doesScaleChangeOverTheTime)
                    {
                        CurveField(ref abilityAoE.scaleChangeOverTime, 1, abilityAoE.duration, "Scale Over Time");
                    }
                    break;
            }
            
            ObjectField(ref abilityAoE.objectWithParticleSystem, "Object With Area Particle System");
            Vector3Field(ref abilityAoE.relativePositionForParticles, "Relative Position To Area");
        }

        private void AbilityMovement(ref AbilityMovement abilityMovement, float aoeDuration)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            EditorGUI.indentLevel--;
            FoldoutField(ref isAbilityMovementFoldoutOpen, "Ability Movement");
            EditorGUI.indentLevel++;

            if (!isAbilityMovementFoldoutOpen)
            {
                return;
            }
            
            //ToggleField(ref abilityMovement.makesBezierCurves, "Makes Bezier Curves");

            for (int i = 0; i < abilityMovement.positions.Count; i++)
            {
                EditorGUILayout.BeginHorizontal("box");

                EditorGUILayout.BeginVertical(GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.9f));

                abilityMovement.positions[i] = Vector3Field(abilityMovement.positions[i], "Next Position");
                abilityMovement.timeBetweenPositions[i] = FloatField(abilityMovement.timeBetweenPositions[i], 0, aoeDuration, "Time To Reach");
                
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.05f));

                if (GUILayout.Button("-", GUILayout.Height(40)))
                {
                    abilityMovement.positions.RemoveAt(i);
                    abilityMovement.timeBetweenPositions.RemoveAt(i);
                }
                
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.EndHorizontal();
            }

            if (!GUILayout.Button("+"))
            {
                return;
            }

            abilityMovement.positions.Add(new Vector3());
            abilityMovement.timeBetweenPositions.Add(0);
        }
    }
}