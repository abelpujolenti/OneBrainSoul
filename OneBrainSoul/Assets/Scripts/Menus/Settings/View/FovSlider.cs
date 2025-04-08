using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Menus.Settings.View
{
    public class FovSlider : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Slider>().value = SettingsManager.Instance.GetFOV();
        }

        public void SetFOV(float fovValue)
        {
            SettingsManager.Instance.SetFOV(fovValue);
            
            if (EventsManager.OnChangeFov == null)
            {
                return;
            }

            EventsManager.OnChangeFov(fovValue);
        }
    }
}