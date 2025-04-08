using Managers;
using UnityEngine;

namespace Menus.Settings.Sound
{
    public class MasterVolumeMute : MonoBehaviour
    {
        [SerializeField] private GameObject _iconAudioMute;

        private void Start()
        {
            SwitchMuteToggleCheckMark(_iconAudioMute, SettingsManager.Instance.IsMasterVolumeMute());
        }

        public void SwitchMasterVolumeMute()
        {
            SettingsManager.Instance.SwitchMasterVolumeMute();
            SwitchMuteToggleCheckMark(_iconAudioMute, SettingsManager.Instance.IsMasterVolumeMute());
        }

        private void SwitchMuteToggleCheckMark(GameObject checkMark, bool isMute)
        {
            checkMark.SetActive(isMute);
        }
    }
}