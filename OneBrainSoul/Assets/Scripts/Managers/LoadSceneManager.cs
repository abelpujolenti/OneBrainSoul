using BULLSHIT;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class LoadSceneManager : MonoBehaviour
    {
        private static LoadSceneManager _instance;

        public static LoadSceneManager Instance => _instance;

        [SerializeField] private int _managersToLoad;
        private int _currentManagersToLoaded;

        private void Awake()
        {
            _instance = this;
        }

        public void ManagerLoaded()
        {
            _currentManagersToLoaded++;

            if (_currentManagersToLoaded != _managersToLoad)
            {
                return;
            }
            
            LoadNextScene();
            
            Destroy(gameObject);
        }

        private void LoadNextScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            CleanConsole cleanConsole = new CleanConsole();
            cleanConsole = null;
        }
    }
}