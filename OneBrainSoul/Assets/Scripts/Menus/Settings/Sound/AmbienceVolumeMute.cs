using Managers;
using UnityEngine;

namespace Menus.Settings.Sound
{
    public class AmbienceVolumeMute : MonoBehaviour
    {
        public void SwitchAmbienceVolumeMute()
        {
            SettingsManager.Instance.SwitchAmbienceVolumeMute();
        }
    }
}