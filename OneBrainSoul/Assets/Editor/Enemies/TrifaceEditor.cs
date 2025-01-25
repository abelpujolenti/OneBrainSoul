using AI.Combat;
using AI.Combat.AbilitySpecs;
using AI.Combat.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Editor.Enemies
{
    [CustomEditor(typeof(TrifaceSpecs))]
    public class TrifaceEditor : EnemyEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            TrifaceSpecs trifaceSpecs = (TrifaceSpecs)target;

            #region Slam
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Slam", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;

            trifaceSpecs.slamTarget = (AbilityTarget)EditorGUILayout.EnumPopup("Ability Target", trifaceSpecs.slamTarget);

            #region Slam Ability Effect
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            trifaceSpecs.slamEffectType =
                (AbilityEffectType)EditorGUILayout.EnumPopup("Ability Effect Type", trifaceSpecs.slamEffectType);

            switch (trifaceSpecs.slamEffectType)
            {
                case AbilityEffectType.DIRECT_DAMAGE:
                    trifaceSpecs.slamEffect.damage =
                        EditorGUILayout.FloatField("Damage", trifaceSpecs.slamEffect.damage);
                    break;
                
                case AbilityEffectType.DAMAGE_PER_DOTS:
                    trifaceSpecs.slamEffect.totalDamage =
                        EditorGUILayout.FloatField("Total Damage", trifaceSpecs.slamEffect.totalDamage);
                    
                    trifaceSpecs.slamEffect.damageDuration =
                        EditorGUILayout.FloatField("Damage Duration", trifaceSpecs.slamEffect.damageDuration);
                    break;
                
                case AbilityEffectType.DIRECT_HEAL:
                    trifaceSpecs.slamEffect.heal =
                        EditorGUILayout.FloatField("Heal", trifaceSpecs.slamEffect.heal);
                    break;
                
                case AbilityEffectType.HEAL_PER_DOTS:
                    trifaceSpecs.slamEffect.totalHeal =
                        EditorGUILayout.FloatField("Total Heal", trifaceSpecs.slamEffect.totalHeal);
                    
                    trifaceSpecs.slamEffect.healDuration =
                        EditorGUILayout.FloatField("Heal Duration", trifaceSpecs.slamEffect.healDuration);
                    break;
                
                case AbilityEffectType.SLOW:
                    trifaceSpecs.slamEffect.slowStrength =
                        EditorGUILayout.FloatField("Slow Strength", trifaceSpecs.slamEffect.slowStrength);
                    break;
            }

            #endregion

            #region Slam Ability Cast
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            trifaceSpecs.slamCast.minimumRangeToCast =
                EditorGUILayout.FloatField("Minimum Range Cast", trifaceSpecs.slamCast.minimumRangeToCast);

            trifaceSpecs.slamCast.maximumRangeToCast =
                EditorGUILayout.FloatField("Maximum Range Cast", trifaceSpecs.slamCast.maximumRangeToCast);

            trifaceSpecs.slamCast.timeToCast =
                EditorGUILayout.FloatField("Time To Cast", trifaceSpecs.slamCast.timeToCast);
            
            trifaceSpecs.slamCast.cooldown =
                EditorGUILayout.FloatField("Cooldown", trifaceSpecs.slamCast.cooldown);

            trifaceSpecs.slamCastType =
                (AbilityCastType)EditorGUILayout.EnumPopup("Ability Cast Type", trifaceSpecs.slamCastType);

            switch (trifaceSpecs.slamCastType)
            {
                case AbilityCastType.OMNIPRESENT:
                    
                    break;
                
                case AbilityCastType.NO_PROJECTILE:
                    trifaceSpecs.slamCast.doesSpawnInsideCaster = EditorGUILayout.Toggle(
                        "Does Relative Position To Caster Change", trifaceSpecs.slamCast.doesSpawnInsideCaster);

                    if (!trifaceSpecs.slamCast.doesSpawnInsideCaster)
                    {
                        trifaceSpecs.slamCast.relativeSpawnPosition =
                            EditorGUILayout.Vector3Field("Relative Position To Caster", trifaceSpecs.slamCast.relativeSpawnPosition);
                    }
                    
                    break;
                
                case AbilityCastType.PARABOLA_PROJECTILE:
                case AbilityCastType.STRAIGHT_LINE_PROJECTILE:
                    trifaceSpecs.slamCast.projectileSpeed =
                        EditorGUILayout.FloatField("Projetile Speed", trifaceSpecs.slamCast.projectileSpeed);
                    break;
            }

            #endregion

            #region Slam Ability AoE
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            trifaceSpecs.slamAoEType =
                (AbilityAoEType)EditorGUILayout.EnumPopup("Ability AoE Type", trifaceSpecs.slamAoEType);

            trifaceSpecs.slamAoE.height = EditorGUILayout.FloatField("Height", trifaceSpecs.slamAoE.height);
            
            switch (trifaceSpecs.slamAoEType)
            {
                case AbilityAoEType.RECTANGLE_AREA:
                    trifaceSpecs.slamAoE.width = EditorGUILayout.FloatField("Width", trifaceSpecs.slamAoE.width);
                    trifaceSpecs.slamAoE.length = EditorGUILayout.FloatField("Length", trifaceSpecs.slamAoE.length);
                    trifaceSpecs.slamAoE.direction = EditorGUILayout.Vector3Field("Direction", trifaceSpecs.slamAoE.direction);
                    break;
                
                case AbilityAoEType.CIRCLE_AREA:
                    trifaceSpecs.slamAoE.radius = EditorGUILayout.FloatField("Radius", trifaceSpecs.slamAoE.radius);
                    break;
                
                case AbilityAoEType.CONE_AREA:
                    trifaceSpecs.slamAoE.length = EditorGUILayout.FloatField("Length", trifaceSpecs.slamAoE.length);
                    trifaceSpecs.slamAoE.direction = EditorGUILayout.Vector3Field("Direction", trifaceSpecs.slamAoE.direction);
                    trifaceSpecs.slamAoE.degrees = EditorGUILayout.FloatField("Degrees", trifaceSpecs.slamAoE.degrees);
                    break;
            }

            #endregion

            #endregion

            if (GUI.changed)
            {
                EditorUtility.SetDirty(trifaceSpecs);
            }
        }
    }
}