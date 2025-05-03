using Managers;
using UnityEngine;

namespace Menus.Settings.Sound
{
    public class AmbienceVolumeMute : MonoBehaviour
    {
        [SerializeField] private GameObject _iconAudioMute;

        private void Start()
        {
            SwitchMuteToggleCheckMark(_iconAudioMute, SettingsManager.Instance.IsAmbienceVolumeMute());
        }
        
        public void SwitchAmbienceVolumeMute()
        {
            SettingsManager.Instance.SwitchAmbienceVolumeMute();
        }

        private void SwitchMuteToggleCheckMark(GameObject checkMark, bool isMute)
        {
            checkMark.SetActive(isMute);
        }
    }
}