using Managers;
using UnityEngine;

namespace Menus.Settings.Sound
{
    public class MusicVolumeMute : MonoBehaviour
    {
        public void SwitchMusicVolumeMute()
        {
            SettingsManager.Instance.SwitchMusicVolumeMute();
        }
    }
}