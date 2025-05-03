using Managers;
using UnityEngine;

namespace Menus.Settings.Sound
{
    public class MusicVolumeMute : MonoBehaviour
    {
        [SerializeField] private GameObject _iconAudioMute;

        private void Start()
        {
            SwitchMuteToggleCheckMark(_iconAudioMute, SettingsManager.Instance.IsMusicVolumeMute());
        }
        
        public void SwitchMusicVolumeMute()
        {
            SettingsManager.Instance.SwitchMusicVolumeMute();
        }

        private void SwitchMuteToggleCheckMark(GameObject checkMark, bool isMute)
        {
            checkMark.SetActive(isMute);
        }
    }
}