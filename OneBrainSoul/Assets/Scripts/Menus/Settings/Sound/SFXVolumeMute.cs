using Managers;
using UnityEngine;

namespace Menus.Settings.Sound
{
    public class SFXVolumeMute : MonoBehaviour
    {
        public void SwitchSFXVolumeMute()
        {
            SettingsManager.Instance.SwitchSFXVolumeMute();
        }
    }
}