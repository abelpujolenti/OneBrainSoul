using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Menus.Settings.Sound
{
    public class SFXVolumeSlider : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Slider>().value = SettingsManager.Instance.GetSFXVolume();
        }

        public void SetSFXVolume(float sfxVolumeValue)
        {
            SettingsManager.Instance.SetSFXVolume(sfxVolumeValue);
        }
    }
}