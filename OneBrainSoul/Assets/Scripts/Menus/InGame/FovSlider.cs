using Managers;
using UnityEngine;

namespace Menus.InGame
{
    public class FovSlider : MonoBehaviour
    {
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