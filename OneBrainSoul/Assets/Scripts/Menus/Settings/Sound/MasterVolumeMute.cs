using Managers;
using UnityEngine;

namespace Menus.Settings.Sound
{
    public class MasterVolumeMute : MonoBehaviour
    {
        public void SwitchMasterVolumeMute()
        {
            SettingsManager.Instance.SwitchMasterVolumeMute();
        }
    }
}