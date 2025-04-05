using Managers;
using UnityEngine;

namespace Menus.InGame
{
    public class BrightnessSlider : MonoBehaviour
    {
        public void SetBrightness(float brightnessValue)
        {
            SettingsManager.Instance.SetBrightness(brightnessValue);
            PostProcessingManager.Instance.SetBrightness(brightnessValue);
        }
    }
}