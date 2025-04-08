using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Menus.InGame
{
    public class InGameMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _buttonsPanel;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private Button _backButton;

        private GameObject _currentActivePanel;
        
        private void Start()
        {
            EventsManager.ReleaseEscape += Pause;
            _currentActivePanel = _buttonsPanel;
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

        public void Resume()
        {
            EventsManager.ReleaseEscape -= Resume;
            EventsManager.ReleaseEscape += Pause;
            AudioManager.Instance.PlayOneShot(FMODEvents.instance.uiExit, transform.position);
            Time.timeScale = 1;
            gameObject.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void Settings()
        {
            //TODO SOUND
            ChangeCurrentActivePanel(_settingsPanel);
        }

        public void MainMenu()
        {
            EventsManager.ReleaseEscape -= Resume;
            AudioManager.Instance.PlayOneShot(FMODEvents.instance.uiSelect, transform.position);
            Time.timeScale = 1;
            LoadSceneManager.Instance.GoToMainMenu();
        }

        public void Exit()
        {
            EventsManager.ReleaseEscape -= Resume;
            AudioManager.Instance.PlayOneShot(FMODEvents.instance.uiExit, transform.position);
            Time.timeScale = 1;
            LoadSceneManager.Instance.ExitGame();
        }

        private void ChangeCurrentActivePanel(GameObject panelToActive) 
        {
            _currentActivePanel.SetActive(false);
            panelToActive.SetActive(true);
            _currentActivePanel = panelToActive;
            _backButton.gameObject.SetActive(_currentActivePanel != _buttonsPanel);
        }

        public void GoBack()
        {
            //TODO SOUND
            ChangeCurrentActivePanel(_buttonsPanel);
        }

        private void OnDestroy()
        {
            Time.timeScale = 1;
            EventsManager.ReleaseEscape -= Pause;
            EventsManager.ReleaseEscape -= Resume;
        }
    }
}