using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Menus.Settings.Sound
{
    public class MusicVolumeSlider : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Slider>().value = SettingsManager.Instance.GetMusicVolume();
        }

        public void SetMusicVolume(float musicVolumeValue)
        {
            SettingsManager.Instance.SetMusicVolume(musicVolumeValue);
        }
    }
}