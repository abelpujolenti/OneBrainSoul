using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class LoadSceneManager : MonoBehaviour
    {
        private static LoadSceneManager _instance;

        public static LoadSceneManager Instance => _instance;

        private const int MAIN_MENU_SCENE_INDEX = 0;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                
                DontDestroyOnLoad(gameObject);
                
                return;
            }
            
            Destroy(gameObject);
        }

        public void LoadNextScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        public void GoToMainMenu()
        {
            SceneManager.LoadScene(MAIN_MENU_SCENE_INDEX);
        }

        public void ExitGame()
        {
            Application.Quit();
        }
    }
}