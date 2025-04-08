using MessagePack;

namespace Serialize
{
    [MessagePackObject]
    public class SerializableSettings
    {
        [Key(0)] public float masterVolume;

        [Key(1)] public float ambienceVolume;

        [Key(2)] public float SFXVolume;

        [Key(3)] public float musicVolume;

        [Key(4)] public bool isMasterVolumeMute;

        [Key(5)] public bool isAmbienceVolumeMute;
        
        [Key(6)] public bool isSFXVolumeMute;
        
        [Key(7)] public bool isMusicVolumeMute;
        
        [Key(8)] public float fov;
        
        [Key(9)] public float brightness;
    }
}