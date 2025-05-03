using Managers;
using UnityEngine;

namespace Menus.Settings.Sound
{
    public class SFXVolumeMute : MonoBehaviour
    {
        [SerializeField] private GameObject _iconAudioMute;

        private void Start()
        {
            SwitchMuteToggleCheckMark(_iconAudioMute, SettingsManager.Instance.IsSFXVolumeMute());
        }
        
        public void SwitchSFXVolumeMute()
        {
            SettingsManager.Instance.SwitchSFXVolumeMute();
        }

        private void SwitchMuteToggleCheckMark(GameObject checkMark, bool isMute)
        {
            checkMark.SetActive(isMute);
        }
    }
}