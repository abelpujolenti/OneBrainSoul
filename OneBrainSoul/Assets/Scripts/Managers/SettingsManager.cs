using UnityEngine;

namespace Managers
{
    public class SettingsManager : MonoBehaviour
    {
        private static SettingsManager _instance;

        public static SettingsManager Instance => _instance;

        private float _masterVolume;
        private float _SFXVolume;
        private float _musicVolume;
        
        private float _fov;
        private float _brightness;

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            
            DontDestroyOnLoad(gameObject);
        }

        public void SetMasterVolume(float masterVolume)
        {
            _masterVolume = masterVolume;
        }

        public void SetSFXVolume(float sfxVolume)
        {
            _SFXVolume = sfxVolume;
        }

        public void SetMusicVolume(float musicVolume)
        {
            _musicVolume = musicVolume;
        }

        public void SetFOV(float fovValue)
        {
            _fov = fovValue;
        }

        public void SetBrightness(float brightnessValue)
        {
            _brightness = brightnessValue;
        }
    }
}