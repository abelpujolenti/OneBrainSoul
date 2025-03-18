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
            AudioManager.instance.PlayOneShot(FMODEvents.instance.uiExit, transform.position);
            Time.timeScale = 1;
            gameObject.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void MainMenu()
        {
            EventsManager.ReleaseEscape -= Resume;
            AudioManager.instance.PlayOneShot(FMODEvents.instance.uiSelect, transform.position);
            Time.timeScale = 1;
            LoadSceneManager.Instance.GoToMainMenu();
        }

        public void Exit()
        {
            EventsManager.ReleaseEscape -= Resume;
            AudioManager.instance.PlayOneShot(FMODEvents.instance.uiExit, transform.position);
            Time.timeScale = 1;
            LoadSceneManager.Instance.ExitGame();
        }

        private void OnDestroy()
        {
            Time.timeScale = 1;
            EventsManager.ReleaseEscape -= Pause;
            EventsManager.ReleaseEscape -= Resume;
        }
    }
}