using Managers;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(ECSNavigationManager)), CanEditMultipleObjects]
    public class NavMeshGraphButtonEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            ECSNavigationManager ecsNavigationManager = (ECSNavigationManager)target;

            if (GUILayout.Button("Bake"))
            {
                ecsNavigationManager.BakeGraph();
            }

            if (GUILayout.Button("Clear"))
            {
                ecsNavigationManager.EraseGraph();
            }
        }
    }
}