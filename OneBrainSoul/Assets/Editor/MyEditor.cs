using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor
{
    public abstract class MyEditor : UnityEditor.Editor
    {
        protected void LabelField(string message)
        {
            EditorGUILayout.LabelField(message, EditorStyles.boldLabel);
        }

        protected void LabelField(string message, int width)
        {
            EditorGUILayout.LabelField(message, EditorStyles.boldLabel, GUILayout.Width(width));
        }

        protected void EnumField<T>(ref T value)
            where T : Enum
        {
            value = (T)EditorGUILayout.EnumPopup(value);
        }

        protected void EnumField<T>(ref T value, string message)
            where T : Enum
        {
            value = (T)EditorGUILayout.EnumPopup(message, value);
        }

        protected void UintField(ref uint value, string message)
        {
            value = (uint)EditorGUILayout.IntField(message, (int)value);
        }

        protected void FloatField(ref float value, string message)
        {
            value = EditorGUILayout.FloatField(message, value);
        }

        protected void Vector3Field(ref Vector3 value, string message)
        {
            value = EditorGUILayout.Vector3Field(message, value);
        }

        protected void ToggleField(ref bool value, string message)
        {
            value = EditorGUILayout.Toggle(message, value);
        }

        protected void ObjectField<T>(ref T value, string message)
            where T : Object
        {
            value = (T)EditorGUILayout.ObjectField(message, value, typeof(T), false);
        }

        protected void UintClamp(ref uint value, uint min, uint max)
        {
            value = Math.Clamp(value, min, max);
        }

        protected void UintMin(ref uint value, uint min)
        {
            value = Math.Min(value, min);
        }

        protected void UintMax(ref uint value, uint max)
        {
            value = Math.Max(value, max);
        }

        protected void FloatClamp(ref float value, float min, float max)
        {
            value = Mathf.Clamp(value, min, max);
        }

        protected void FloatMin(ref float value, float min)
        {
            value = Mathf.Min(value, min);
        }

        protected void FloatMax(ref float value, float max)
        {
            value = Mathf.Max(value, max);
        }
    }
}