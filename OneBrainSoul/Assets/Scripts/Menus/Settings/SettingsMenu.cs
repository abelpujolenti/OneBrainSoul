using Managers;
using UnityEngine;

namespace Menus.Settings
{
    public class SettingsMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _soundSettingsPanel;
        [SerializeField] private GameObject _viewSettingsPanel;
        [SerializeField] private GameObject _controlSettingsPanel;

        private GameObject _currentActivePanel;

        private void Start()
        {
            _currentActivePanel = _soundSettingsPanel;
        }

        public void ActivateSoundPanel()
        {
            ChangeCurrentActivePanel(_soundSettingsPanel);
        }

        public void ActivateViewPanel()
        {
            ChangeCurrentActivePanel(_viewSettingsPanel);
        }

        public void ActivateControlsPanel()
        {
            ChangeCurrentActivePanel(_controlSettingsPanel);
        }

        private void ChangeCurrentActivePanel(GameObject panelToActive) 
        {
            _currentActivePanel.SetActive(false);
            panelToActive.SetActive(true);
            _currentActivePanel = panelToActive;
        }
    }
}