using System;
using System.Reflection;
using UnityEditor;

namespace BULLSHIT
{
    public class CleanConsole
    {
        public CleanConsole()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
            Type logEntries = assembly.GetType("UnityEditor.LogEntries");
            MethodInfo clearMethod = logEntries.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
            clearMethod.Invoke(null, null);
        }
    }
}