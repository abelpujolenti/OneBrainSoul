using AI.Combat.AbilitySpecs;
using AI.Combat.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Editor.Enemies
{
    [CustomEditor(typeof(AgentAbility))]
    public class AgentAbilityEditor : MyEditor
    {
        private bool areAbilityConditionsFoldoutOpen = true;
        private bool areAnimationParametersFoldoutOpen = true;
        
        public override void OnInspectorGUI()
        {
            AgentAbility agentAbility = (AgentAbility)target;
            
            AbilityTarget(agentAbility);
            
            AbilityEffect(agentAbility);
            
            AbilityCast(agentAbility);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(agentAbility);
            }
        }

        private void AbilityTarget(AgentAbility agentAbility)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            LabelField("Ability Target", 150);
            EnumField<AbilityTarget>(ref agentAbility.abilityTarget);
            EditorGUILayout.EndHorizontal();
        }

        private void AbilityEffect(AgentAbility agentAbility)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            LabelField("Ability Effect");
            EditorGUI.indentLevel++;
            
            EnumField<AbilityEffectType>(ref agentAbility.abilityEffectType, "Ability Effect Type");

            switch (agentAbility.abilityEffectType)
            {
                case AbilityEffectType.DIRECT_DAMAGE:
                    
                    UintField(ref agentAbility.abilityEffect.value, "Damage");
                    UintMax(ref agentAbility.abilityEffect.value, 0);
                    agentAbility.abilityEffect.duration = 0;
                    
                    break;
                
                case AbilityEffectType.DAMAGE_PER_TICKS:
                    
                    UintField(ref agentAbility.abilityEffect.value, "Damage per Tick");
                    UintMax(ref agentAbility.abilityEffect.value, 0);
                    
                    FloatField(ref agentAbility.abilityEffect.duration, "Damage Duration");
                    FloatMax(ref agentAbility.abilityEffect.duration, 0);
                    
                    break;
                
                case AbilityEffectType.DIRECT_HEAL:
                    
                    UintField(ref agentAbility.abilityEffect.value, "Heal");
                    UintMax(ref agentAbility.abilityEffect.value, 0);
                    agentAbility.abilityEffect.duration = 0;
                    
                    break;
                
                case AbilityEffectType.HEAL_PER_TICKS:
                    
                    UintField(ref agentAbility.abilityEffect.value, "Heal per Tick");
                    UintMax(ref agentAbility.abilityEffect.value, 0);
                    
                    FloatField(ref agentAbility.abilityEffect.duration, "Heal Duration");
                    FloatMax(ref agentAbility.abilityEffect.duration, 0);
                    
                    break;
                
                case AbilityEffectType.SLOW:
                    
                    UintField(ref agentAbility.abilityEffect.value, "Slow Strength");
                    UintClamp(ref agentAbility.abilityEffect.value, 0, 100);
                    
                    FloatField(ref agentAbility.abilityEffect.duration, "Slow Duration");
                    FloatMax(ref agentAbility.abilityEffect.duration, 0);
                    
                    break;
            }
        }

        private void AbilityCast(AgentAbility agentAbility)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            EditorGUI.indentLevel--;
            LabelField("Ability Cast");
            EditorGUI.indentLevel++;
            
            FloatField(ref agentAbility.abilityCast.minimumRangeToCast, "Minimum Range To Cast");
            FloatMax(ref agentAbility.abilityCast.minimumRangeToCast, 0);
            
            FloatField(ref agentAbility.abilityCast.maximumRangeToCast, "Maximum Range To Cast");
            FloatMax(ref agentAbility.abilityCast.maximumRangeToCast, 0);
            
            FloatField(ref agentAbility.abilityCast.timeToCast, "Time To Cast");
            FloatMax(ref agentAbility.abilityCast.timeToCast, 0);
            
            FloatField(ref agentAbility.abilityCast.cooldown, "Cooldown");
            FloatMax(ref agentAbility.abilityCast.cooldown, 0);
            
            EnumField<AbilityCastType>(ref agentAbility.abilityCastType, "Ability Cast Type");

            switch (agentAbility.abilityCastType)
            {
                case AbilityCastType.OMNIPRESENT:
                    
                    break;
                
                case AbilityCastType.NO_PROJECTILE:

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
                    
                    AbilityAoE(agentAbility);
                    
                    break;
                
                case AbilityCastType.PARABOLA_PROJECTILE:
                case AbilityCastType.STRAIGHT_LINE_PROJECTILE:
                    
                    FloatField(ref agentAbility.abilityCast.projectileSpeed, "Projectile Speed");
                    FloatMax(ref agentAbility.abilityCast.projectileSpeed, 0);

                    if (!agentAbility.abilityCast.doesSpawnInsideCaster)
                    {
                        agentAbility.abilityCast.relativeSpawnPosition = Vector3.zero;
                    }
                    else
                    {
                        Vector3Field(ref agentAbility.abilityCast.relativeSpawnPosition, "Relative Position To Caster");
                    }

                    ToggleField(ref agentAbility.abilityCast.isAttachedToCaster, "Is Attached To Caster");
                    
                    AbilityProjectile(agentAbility);
                    AbilityAoE(agentAbility);
                    
                    break;
            }
        }

        private void AbilityAoE(AgentAbility agentAbility)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            EditorGUI.indentLevel--;
            LabelField("Ability AoE");
            EditorGUI.indentLevel++;
            
            EnumField<AbilityAoEType>(ref agentAbility.abilityAoEType, "Ability AoE Type");

            FloatField(ref agentAbility.abilityAoE.height, "Height");
            FloatMax(ref agentAbility.abilityAoE.height, 0);
            
            switch (agentAbility.abilityAoEType)
            {
                case AbilityAoEType.RECTANGLE_AREA:
                    
                    FloatField(ref agentAbility.abilityAoE.width, "Width");
                    FloatMax(ref agentAbility.abilityAoE.width, 0);
                    
                    FloatField(ref agentAbility.abilityAoE.length, "Length");
                    FloatMax(ref agentAbility.abilityAoE.length, 0);
                    
                    Vector3Field(ref agentAbility.abilityAoE.direction, "Direction");
                    
                    break;
                
                case AbilityAoEType.CIRCLE_AREA:
                    
                    FloatField(ref agentAbility.abilityAoE.radius, "Radius");
                    FloatMax(ref agentAbility.abilityAoE.radius, 0);
                    
                    break;
                
                case AbilityAoEType.CONE_AREA:
                    FloatField(ref agentAbility.abilityAoE.length, "Length");
                    FloatMax(ref agentAbility.abilityAoE.length, 0);
                    
                    Vector3Field(ref agentAbility.abilityAoE.direction, "Direction");
                    
                    FloatField(ref agentAbility.abilityAoE.degrees, "Degrees");
                    FloatClamp(ref agentAbility.abilityAoE.degrees, 0, 360);
                    
                    break;
            }
        }

        private void AbilityProjectile(AgentAbility agentAbility)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            EditorGUI.indentLevel--;
            LabelField("Ability Projectile");
            EditorGUI.indentLevel++;
            
            ObjectField<GameObject>(ref agentAbility.projectilePrefab, "Projectile Prefab");

            EnumField(ref agentAbility.abilityProjectileType, "Projectile Type");

            switch (agentAbility.abilityProjectileType)
            {
                case AbilityProjectileType.VANISH_ON_IMPACT:
                    break;
                
                case AbilityProjectileType.VANISH_OVER_TIME:
                    
                    FloatField(ref agentAbility.abilityProjectile.timeToVanish, "Time To Vanish");
                    FloatMax(ref agentAbility.abilityProjectile.timeToVanish, 0);
                    
                    break;
            }
        }
    }
}