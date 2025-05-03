using UnityEngine;

namespace Menus.Settings
{
    public class SettingsMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _soundSettingsPanel;
        [SerializeField] private GameObject _viewSettingsPanel;
        [SerializeField] private GameObject _controlSettingsPanel;

        [SerializeField] private TextButton _soundSettingsButton;
        [SerializeField] private TextButton _viewSettingsButton;
        [SerializeField] private TextButton _controlSettingsButton;

        private GameObject _currentActivePanel;
        private TextButton _currentSelectedButton;

        private void Start()
        {
            _currentActivePanel = _soundSettingsPanel;
            _currentSelectedButton = _soundSettingsButton;
            _soundSettingsButton.ShowAsSelected();
        }

        public void ActivateSoundPanel()
        {
            ChangeCurrentActivePanel(_soundSettingsPanel);
            ChangeCurrentSelectedButton(_soundSettingsButton);
        }

        public void ActivateViewPanel()
        {
            ChangeCurrentActivePanel(_viewSettingsPanel);
            ChangeCurrentSelectedButton(_viewSettingsButton);
        }

        public void ActivateControlsPanel()
        {
            ChangeCurrentActivePanel(_controlSettingsPanel);
            ChangeCurrentSelectedButton(_controlSettingsButton);
        }

        private void ChangeCurrentActivePanel(GameObject panelToActive) 
        {
            _currentActivePanel.SetActive(false);
            panelToActive.SetActive(true);
            _currentActivePanel = panelToActive;
        }

        private void ChangeCurrentSelectedButton(TextButton textButton)
        {
            _currentSelectedButton.ShowAsNormal();
            textButton.ShowAsSelected();
            _currentSelectedButton = textButton;
        }
    }
}