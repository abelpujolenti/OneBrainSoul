using Managers;
using UnityEngine;

namespace Menus.EndScreen
{
    public class EndScreen : MonoBehaviour
    {
        public void MainMenu()
        {
            LoadSceneManager.Instance.GoToMainMenu();
        }
    }
}