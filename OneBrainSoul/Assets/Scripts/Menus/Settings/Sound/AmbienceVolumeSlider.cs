using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Menus.Settings.Sound
{
    public class AmbienceVolumeSlider : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Slider>().value = SettingsManager.Instance.GetAmbienceVolume();
        }

        public void SetAmbienceVolume(float ambienceVolumeValue)
        {
            SettingsManager.Instance.SetAmbienceVolume(ambienceVolumeValue);
        }
    }
}