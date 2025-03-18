using Managers;
using UnityEngine;

namespace Menus.InGame
{
    public class InGameMenu : MonoBehaviour
    {
        private void Start()
        {
            EventsManager.ReleaseEscape += Pause;
            gameObject.SetActive(false);
        }

        private void Pause()
        {
            EventsManager.ReleaseEscape -= Pause;
            EventsManager.ReleaseEscape += Resume;
            Time.timeScale = 0;
            gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
        }

        private void Resume()
        {
            EventsManager.ReleaseEscape -= Resume;
            EventsManager.ReleaseEscape += Pause;
            Time.timeScale = 1;
            gameObject.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void MainMenu()
        {
            EventsManager.ReleaseEscape -= Resume;
            LoadSceneManager.Instance.GoToMainMenu();
        }

        public void Exit()
        {
            EventsManager.ReleaseEscape -= Resume;
            LoadSceneManager.Instance.ExitGame();
        }

        private void OnDestroy()
        {
            EventsManager.ReleaseEscape -= Pause;
            EventsManager.ReleaseEscape -= Resume;
        }
    }
}