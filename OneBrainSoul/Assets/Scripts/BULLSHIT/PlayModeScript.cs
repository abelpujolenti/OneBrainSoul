#if UNITY_EDITOR

using UnityEditor;
using UnityEngine.SceneManagement;

namespace BULLSHIT
{
    [InitializeOnLoad]
    public static class PlayModeScript
    {
        static PlayModeScript()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName("ControllerTest"))
            {
                return;
            }
            
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                SceneManager.LoadScene("LoadScene");
            }
        }

    }
}

#endif