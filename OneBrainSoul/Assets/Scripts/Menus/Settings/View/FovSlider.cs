using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menus.Settings.View
{
    public class FovSlider : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        
        private void Start()
        {
            float value = SettingsManager.Instance.GetFOV();
            GetComponent<Slider>().value = value;
            _text.text = value + "";
        }

        public void SetFOV(float fovValue)
        {
            SettingsManager.Instance.SetFOV(fovValue);
            _text.text = fovValue + "";
            
            if (EventsManager.OnChangeFov == null)
            {
                return;
            }

            EventsManager.OnChangeFov(fovValue);
        }
    }
}