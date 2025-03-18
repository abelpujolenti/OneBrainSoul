using Managers;
using UnityEngine;

namespace Menus.EndScreen
{
    public class EndScreen : MonoBehaviour
    {
        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
        }

        public void MainMenu()
        {
            LoadSceneManager.Instance.GoToMainMenu();
        }
    }
}