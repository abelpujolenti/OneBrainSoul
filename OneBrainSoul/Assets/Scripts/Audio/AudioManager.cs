using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class AudioManager : MonoBehaviour
{
    private Bus masterBus;
    private Bus musicBus;
    private Bus ambienceBus;
    private Bus sfxBus;

    private List<EventInstance> eventInstances;
    private List<StudioEventEmitter> eventEmitters;

    private EventInstance ambienceEventInstance;
    private EventInstance ambienceAdditionsEventInstance;
    private EventInstance musicEventInstance;
    private EventInstance insideSnapshotEventInstance;

    private EventInstance lowHealthEventInstance;
    private EventInstance ghostModeEventInstance;

    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        eventInstances = new List<EventInstance>();
        eventEmitters = new List<StudioEventEmitter>();

        masterBus = RuntimeManager.GetBus("bus:/");
        musicBus = RuntimeManager.GetBus("bus:/Music");
        ambienceBus = RuntimeManager.GetBus("bus:/Ambience");
        sfxBus = RuntimeManager.GetBus("bus:/SFX");
    }

    private void Start()
    {
        //InitializeMusic();
        //InitializeAmbience();
        /*InitializeSnapshots();*/

        SceneManager.sceneLoaded += OnSceneLoad;
        OnSceneLoad(SceneManager.GetActiveScene(), LoadSceneMode.Single);

        InitializeGhostMode();
        InitializeLowHealth();
    }

    private void OnSceneLoad(Scene s, LoadSceneMode mode)
    {
        if (s.name == "MainMenu")
        {
            InitializeAmbience();
        }
        else if (s.name == "ControllerTest")
        {
            InitializeMusic();
        }
    }

    public void SetMasterVolume(float masterVolume)
    {
        masterBus.setVolume(masterVolume);
    }

    public void SetAmbienceVolume(float ambienceVolume)
    {
        ambienceBus.setVolume(ambienceVolume);
    }

    public void SetSFXVolume(float sfxVolume)
    {
        sfxBus.setVolume(sfxVolume);
    }

    public void SetMusicVolume(float musicVolume)
    {
        musicBus.setVolume(musicVolume);
    }

    public void InitializeAmbience()
    {
        PLAYBACK_STATE s;
        ambienceEventInstance.getPlaybackState(out s);
        if (s == PLAYBACK_STATE.STARTING || s == PLAYBACK_STATE.PLAYING) return;
        ambienceEventInstance = CreateInstance(FMODEvents.instance.ambient);
        ambienceEventInstance.start();
    }
/*
    private void InitializeSnapshots()
    {
        insideSnapshotEventInstance = CreateInstance(FMODEvents.instance.insideSnapshot);
        insideSnapshotEventInstance.start();
    }*/
    public void InitializeMusic()
    {
        PLAYBACK_STATE s;
        musicEventInstance.getPlaybackState(out s);
        if (s == PLAYBACK_STATE.STARTING || s == PLAYBACK_STATE.PLAYING) return;
        musicEventInstance = CreateInstance(FMODEvents.instance.music);
        musicEventInstance.start();
    }

    public void InitializeLowHealth()
    {
        PLAYBACK_STATE s;
        lowHealthEventInstance.getPlaybackState(out s);
        if (s == PLAYBACK_STATE.STARTING || s == PLAYBACK_STATE.PLAYING) return;
        lowHealthEventInstance = CreateInstance(FMODEvents.instance.lowHealth);
        lowHealthEventInstance.start();
        StopLowHealth();
    }

    public void InitializeGhostMode()
    {
        PLAYBACK_STATE s;
        ghostModeEventInstance.getPlaybackState(out s);
        if (s == PLAYBACK_STATE.STARTING || s == PLAYBACK_STATE.PLAYING) return;
        ghostModeEventInstance = CreateInstance(FMODEvents.instance.ghostMode);
        ghostModeEventInstance.start();
        StopGhostMode();
    }

    public void PlayLowHealth()
    {
        lowHealthEventInstance.setPaused(false);
    }
    public void StopLowHealth()
    {
        lowHealthEventInstance.setPaused(true);
    }

    public void PlayGhostMode()
    {
        ghostModeEventInstance.setPaused(false);
    }
    public void StopGhostMode()
    {
        ghostModeEventInstance.setPaused(true);
    }

    public void SetMusicParameter(string parameterName, float parameterValue)
    {
        musicEventInstance.setParameterByName(parameterName, parameterValue);
    }

    public float GetMusicParameter(string parameterName)
    {
        float f;
        musicEventInstance.getParameterByName(parameterName, out f);
        return f;
    }

    public void PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        if (sound.IsNull) return;
        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    public EventInstance CreateInstance(EventReference eventReference)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        eventInstances.Add(eventInstance);
        return eventInstance;
    }

    public StudioEventEmitter InitializeEventEmitter(EventReference eventReference, GameObject emitterGameObject)
    {
        StudioEventEmitter emitter = emitterGameObject.GetComponent<StudioEventEmitter>();
        emitter.EventReference = eventReference;
        eventEmitters.Add(emitter);
        return emitter;
    }

    private void CleanUp()
    {
        // stop and release any created instances
        foreach (EventInstance eventInstance in eventInstances)
        {
            eventInstance.stop(STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }
        // stop all of the event emitters, because if we don't they may hang around in other scenes
        foreach (StudioEventEmitter emitter in eventEmitters)
        {
            emitter.Stop();
        }
    }

    private void OnDestroy()
    {
        CleanUp();
    }
}
