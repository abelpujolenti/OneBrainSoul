using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Menus.Settings.Sound
{
    public class MasterVolumeSlider : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Slider>().value = SettingsManager.Instance.GetMasterVolume();
        }

        public void SetMasterVolume(float masterVolumeValue)
        {
            SettingsManager.Instance.SetMasterVolume(masterVolumeValue);
        }
    }
}