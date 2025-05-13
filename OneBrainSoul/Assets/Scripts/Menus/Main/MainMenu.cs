using System.Collections;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Menus.Main
{
    public class MainMenu : MonoBehaviour
    {
        private static MainMenu _instance;

        public static MainMenu Instance => _instance;

        [SerializeField] private float _delayBeforeShowPressAnyKey;
        [SerializeField] private float _autoScrollTime;

        [SerializeField] private ScrollRect _scrollRect;

        [SerializeField] private float _positionToScroll;

        [SerializeField] private PressAnyKey _pressAnyKey;

        [SerializeField] private RectTransform _contentToScroll;

        [SerializeField] private GameObject _buttonsPanel;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _creditsPanel;
        [SerializeField] private Button _backButton;
        
        private GameObject _currentActivePanel;

        [SerializeField] private GameObject[] _buttons;

        private void Awake()
        {
            _instance = this;
            _currentActivePanel = _buttonsPanel;
        }

        public void TitleAnimationFinished()
        {
            StartCoroutine(DelayAfterTitleAnimationFinished());
        }

        private IEnumerator DelayAfterTitleAnimationFinished()
        {
            float timer = 0;

            while (timer < _delayBeforeShowPressAnyKey)
            {
                timer += Time.deltaTime;

                yield return null;
            }
            
            _pressAnyKey.gameObject.SetActive(true);
        }

        public void ScrollDown()
        {
            StartCoroutine(ScrollDownCoroutine());
        }

        private IEnumerator ScrollDownCoroutine()
        {
            float timer = 0;

            float startYPosition = _contentToScroll.transform.localPosition.y;
            float endYPosition = _positionToScroll;
            
            while (timer < _autoScrollTime)
            {
                timer += Time.deltaTime;
                _contentToScroll.transform.localPosition =
                    new Vector3(0, Mathf.SmoothStep(startYPosition, endYPosition, timer / _autoScrollTime), 0);
                yield return null;
            }

            _scrollRect.vertical = true;

            foreach (GameObject button in _buttons)
            {
                button.SetActive(true);
            }
        }

        public void Play()
        {
            AudioManager.Instance.PlayOneShot(FMODEvents.instance.uiGameStart, transform.position);
            LoadSceneManager.Instance.LoadNextScene();
        }

        public void Settings()
        {
            //TODO SOUND
            ChangeCurrentActivePanel(_settingsPanel);
        }

        public void Credits()
        {
            //TODO SOUND
            ChangeCurrentActivePanel(_creditsPanel);
        }

        public void Exit()
        {
            AudioManager.Instance.PlayOneShot(FMODEvents.instance.uiExit, transform.position);
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
            _instance = null;
        }
    }
}