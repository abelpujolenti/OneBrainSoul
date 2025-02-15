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