using BULLSHIT;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class LoadManager : MonoBehaviour
    {
        private static LoadManager _instance;

        public static LoadManager Instance => _instance;

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
#if UNITY_EDITOR
            CleanConsole cleanConsole = new CleanConsole();
            cleanConsole = null;
#endif
        }
    }
}