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
        }

        private void Resume()
        {
            EventsManager.ReleaseEscape -= Resume;
            EventsManager.ReleaseEscape += Pause;
            Time.timeScale = 1;
            gameObject.SetActive(false);
        }

        public void MainMenu()
        {
            EventsManager.ReleaseEscape -= Resume;
            GameManager.Instance.GoToMainMenu();
        }

        public void Exit()
        {
            EventsManager.ReleaseEscape -= Resume;
            GameManager.Instance.ExitGame();
        }

        private void OnDestroy()
        {
            EventsManager.ReleaseEscape -= Pause;
            EventsManager.ReleaseEscape -= Resume;
        }
    }
}