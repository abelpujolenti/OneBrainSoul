﻿using AI.Combat.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(AIAllyAttack))]
    public class DisplayAIAllyAttackFieldsBySelection : DisplayAIAttackFieldsBySelection
    {
        public override void OnInspectorGUI()
        {
            AIAllyAttack aiAllyAttack = (AIAllyAttack)target;

            aiAllyAttack.stressDamage = EditorGUILayout.FloatField("Stress Damage", aiAllyAttack.stressDamage);
            base.OnInspectorGUI();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(aiAllyAttack);
            }
        }
    }
}