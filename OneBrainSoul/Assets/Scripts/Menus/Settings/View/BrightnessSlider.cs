using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Menus.Settings.View
{
    public class BrightnessSlider : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Slider>().value = SettingsManager.Instance.GetBrightness();
        }

        public void SetBrightness(float brightnessValue)
        {
            SettingsManager.Instance.SetBrightness(brightnessValue);
        }
    }
}