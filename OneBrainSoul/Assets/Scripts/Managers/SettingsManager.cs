using System.IO;
using MessagePack;
using Serialize;
using UnityEngine;

namespace Managers
{
    public class SettingsManager : MonoBehaviour
    {
        private static SettingsManager _instance;

        public static SettingsManager Instance => _instance;

        private float _masterVolume;
        private float _ambienceVolume;
        private float _SFXVolume;
        private float _musicVolume;

        private bool _isMasterVolumeMute;
        private bool _isAmbienceVolumeMute;
        private bool _isSFXVolumeMute;
        private bool _isMusicVolumeMute;
        
        private float _fov;
        private float _brightness;

        private void Start()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            
            LoadSettingFile();
            
            DontDestroyOnLoad(gameObject);
        }

        private void LoadSettingFile()
        {
            if (!File.Exists(GetFilePath()))
            {
                Debug.LogError("Missing Settings File");
                return;
            }

            byte[] data = File.ReadAllBytes(GetFilePath());

            SerializableSettings serializableSettings = MessagePackSerializer.Deserialize<SerializableSettings>(data);

            SetMasterVolume(serializableSettings.masterVolume);
            SetAmbienceVolume(serializableSettings.ambienceVolume);
            SetSFXVolume(serializableSettings.SFXVolume);
            SetMusicVolume(serializableSettings.musicVolume);

            if (serializableSettings.isMasterVolumeMute)
            {
                SwitchMasterVolumeMute();
            }

            if (serializableSettings.isAmbienceVolumeMute)
            {
                SwitchAmbienceVolumeMute();
            }

            if (serializableSettings.isSFXVolumeMute)
            {
                SwitchSFXVolumeMute();
            }

            if (serializableSettings.isMusicVolumeMute)
            {
                SwitchMusicVolumeMute();
            }
            
            _fov = serializableSettings.fov;
            SetBrightness(serializableSettings.brightness);
        }

        public void SetMasterVolume(float masterVolume)
        {
            _masterVolume = masterVolume;
            AudioManager.Instance.SetMasterVolume(_masterVolume);
        }

        public float GetMasterVolume()
        {
            return _masterVolume;
        }

        public void SetAmbienceVolume(float ambienceVolume)
        {
            _ambienceVolume = ambienceVolume;
            AudioManager.Instance.SetAmbienceVolume(_ambienceVolume);
        }

        public float GetAmbienceVolume()
        {
            return _ambienceVolume;
        }

        public void SetSFXVolume(float sfxVolume)
        {
            _SFXVolume = sfxVolume;
            AudioManager.Instance.SetSFXVolume(_SFXVolume);
        }

        public float GetSFXVolume()
        {
            return _SFXVolume;
        }

        public void SetMusicVolume(float musicVolume)
        {
            _musicVolume = musicVolume;
            AudioManager.Instance.SetMusicVolume(_musicVolume);
        }

        public float GetMusicVolume()
        {
            return _musicVolume;
        }

        public void SwitchMasterVolumeMute()
        {
            _isMasterVolumeMute ^= true;
            
            if (_isMasterVolumeMute)
            {
                AudioManager.Instance.SetMasterVolume(0);
                return;
            }
            AudioManager.Instance.SetMasterVolume(_masterVolume);
        }

        public bool IsMasterVolumeMute()
        {
            return _isMasterVolumeMute;
        }

        public void SwitchAmbienceVolumeMute()
        {
            _isAmbienceVolumeMute ^= true;
            
            if (_isAmbienceVolumeMute)
            {
                AudioManager.Instance.SetAmbienceVolume(0);
                return;
            }
            AudioManager.Instance.SetAmbienceVolume(_ambienceVolume);
        }

        public bool IsAmbienceVolumeMute()
        {
            return _isAmbienceVolumeMute;
        }

        public void SwitchSFXVolumeMute()
        {
            _isSFXVolumeMute ^= true;
            
            if (_isSFXVolumeMute)
            {
                AudioManager.Instance.SetSFXVolume(0);
                return;
            }
            AudioManager.Instance.SetSFXVolume(_SFXVolume);
        }

        public bool IsSFXVolumeMute()
        {
            return _isSFXVolumeMute;
        }

        public void SwitchMusicVolumeMute()
        {
            _isMusicVolumeMute ^= true;
            
            if (_isMusicVolumeMute)
            {
                AudioManager.Instance.SetMusicVolume(0);
                return;
            }
            AudioManager.Instance.SetMusicVolume(_musicVolume);
        }

        public bool IsMusicVolumeMute()
        {
            return _isMusicVolumeMute;
        }

        public void SetFOV(float fovValue)
        {
            _fov = fovValue;
        }

        public float GetFOV()
        {
            return _fov;
        }

        public void SetBrightness(float brightnessValue)
        {
            _brightness = brightnessValue;
            PostProcessingManager.Instance.SetBrightness(brightnessValue);
        }

        public float GetBrightness()
        {
            return _brightness;
        }

        private void OnDestroy()
        {
            SaveSettingsFile();
        }

        private void SaveSettingsFile()
        {
            SerializableSettings serializableSettings = new SerializableSettings();

            serializableSettings.masterVolume = _masterVolume;
            serializableSettings.ambienceVolume = _ambienceVolume;
            serializableSettings.SFXVolume = _SFXVolume;
            serializableSettings.musicVolume = _musicVolume;
            serializableSettings.isMasterVolumeMute = _isMasterVolumeMute;
            serializableSettings.isAmbienceVolumeMute = _isAmbienceVolumeMute;
            serializableSettings.isSFXVolumeMute = _isSFXVolumeMute;
            serializableSettings.isMusicVolumeMute = _isMusicVolumeMute;
            serializableSettings.fov = _fov;
            serializableSettings.brightness = _brightness;

            byte[] data = MessagePackSerializer.Serialize(serializableSettings);
            File.WriteAllBytes(GetFilePath(), data);
        }

        private string GetFilePath()
        {
            return Application.streamingAssetsPath + "/Settings.json";
        }
    }
}