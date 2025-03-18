using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor
{
    public abstract class MyEditor : UnityEditor.Editor
    {
        private GUIStyle boldFoldoutSyle;

        protected void InitializeStyles()
        {
            if (boldFoldoutSyle != null)
            {
                return;
            }
            
            boldFoldoutSyle = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold
            };   
        }

        private Rect GetControlRect()
        {
            Rect controlRect = EditorGUILayout.GetControlRect();
            return EditorGUI.IndentedRect(controlRect);
        }

        protected void LabelField(string message)
        {
            EditorGUILayout.LabelField(message, EditorStyles.boldLabel);
        }

        protected void LabelField(string message, int width)
        {
            EditorGUILayout.LabelField(message, EditorStyles.boldLabel, GUILayout.Width(width));
        }

        protected void FoldoutField(ref bool isFoldoutOpen, string message)
        {
            isFoldoutOpen = EditorGUILayout.Foldout(isFoldoutOpen, message, true, boldFoldoutSyle);
        }

        protected void EnumField<T>(ref T value)
            where T : Enum
        {
            value = (T)EditorGUI.EnumPopup(GetControlRect(), value);
        }

        protected void EnumField<T>(ref T value, float widthPercentage)
            where T : Enum
        {
            Rect rect = GetControlRect();
            float inspectorWidth = EditorGUIUtility.currentViewWidth;

            float width = inspectorWidth * widthPercentage;
            rect.x += rect.width - width;
            rect.width = width;
            
            value = (T)EditorGUI.EnumPopup(rect, value);
        }

        protected void EnumField<T>(ref T value, string message)
            where T : Enum
        {
            value = (T)EditorGUI.EnumPopup(GetControlRect(), message, value);
        }

        protected void EnumField<T>(ref T value, string message, float widthPercentage)
            where T : Enum
        {
            Rect rect = GetControlRect();
            float inspectorWidth = EditorGUIUtility.currentViewWidth;

            float width = inspectorWidth * widthPercentage;
            rect.x += rect.width - width;
            rect.width = width;
            
            value = (T)EditorGUI.EnumPopup(rect, message, value);
        }

        protected void EnumFlagsField<T>(ref T value, string message)
            where T : Enum
        {
            value = (T)EditorGUI.EnumFlagsField(GetControlRect(), message, value);
        }

        protected void EnumFlagsField<T>(ref T value, float widthPercentage)
            where T : Enum
        {
            Rect rect = GetControlRect();
            float inspectorWidth = EditorGUIUtility.currentViewWidth;

            float width = inspectorWidth * widthPercentage;
            rect.x += rect.width - width;
            rect.width = width;
            
            value = (T)EditorGUI.EnumFlagsField(rect, value);
        }

        protected void UintField(ref uint value, uint min, string message)
        {
            int intValue = EditorGUI.IntField(GetControlRect(), message, (int)value);
            value = (uint)Mathf.Max(min, (uint)intValue);
        }

        protected void UintField(ref uint value, uint min, uint max, string message)
        {
            int intValue = EditorGUI.IntField(GetControlRect(), message, (int)value);
            value = (uint)Mathf.Clamp(intValue, min, max);
        }

        protected void FloatField(ref float value, string message)
        {
            value = EditorGUI.FloatField(GetControlRect(), message, value);
        }

        protected void FloatField(ref float value, float min, string message)
        {
            float floatValue = EditorGUI.FloatField(GetControlRect(), message, value);
            value = Mathf.Max(min, floatValue);
        }

        protected void FloatField(ref float value, float min, float max, string message)
        {
            float floatValue = EditorGUI.FloatField(GetControlRect(), message, value);
            value = Mathf.Clamp(floatValue, min, max);
        }

        protected float FloatField(float value, string message)
        {
            return EditorGUI.FloatField(GetControlRect(), message, value);
        }

        protected float FloatField(float value, float min, string message)
        {
            float floatValue = EditorGUI.FloatField(GetControlRect(), message, value);
            return Mathf.Max(min, floatValue);
        }

        protected float FloatField(float value, float min, float max, string message)
        {
            float floatValue = EditorGUI.FloatField(GetControlRect(), message, value);
            return Mathf.Clamp(floatValue, min, max);
        }

        protected void Vector3Field(ref Vector3 value, string message)
        {
            value = EditorGUI.Vector3Field(GetControlRect(), message, value);
        }

        protected Vector3 Vector3Field(Vector3 value, string message)
        {
            return EditorGUI.Vector3Field(GetControlRect(), message, value);
        }

        protected void ToggleField(ref bool value, string message)
        {
            value = EditorGUI.Toggle(GetControlRect(), message, value);
        }

        protected void CurveField(ref AnimationCurve animationCurve, float initialValue, float maxTime, string message)
        {
            Keyframe[] keys = animationCurve.keys;

            for (int i = 0; i < keys.Length; i++)
            {
                if (i == 0)
                {
                    keys[i].time = 0;
                    keys[i].value = initialValue;
                    continue;
                }
                keys[i].time = Mathf.Clamp(keys[i].time, 0, maxTime);
                keys[i].value = Mathf.Max(keys[i].value, 0);
            }

            animationCurve.keys = keys;
            
            animationCurve = EditorGUI.CurveField(GetControlRect(), message, animationCurve);
        }

        protected void ObjectField<T>(ref T value)
            where T : Object
        {
            value = (T)EditorGUI.ObjectField(GetControlRect(), value, typeof(T), false);
        }

        protected void ObjectField<T>(ref T value, string message)
            where T : Object
        {
            value = (T)EditorGUI.ObjectField(GetControlRect(), message, value, typeof(T), false);
        }

        protected void PropertyField(SerializedProperty serializedProperty, string message)
        {
            EditorGUILayout.PropertyField(serializedProperty, new GUIContent(message));
        }
    }
}