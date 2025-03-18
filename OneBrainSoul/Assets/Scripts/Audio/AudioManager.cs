using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class AudioManager : MonoBehaviour
{
    [Header("Volume")]
    [Range(0, 1)]
    public float masterVolume = 1;
    [Range(0, 1)]
    public float musicVolume = 1;
    [Range(0, 1)]
    public float ambienceVolume = 1;
    [Range(0, 1)]
    public float SFXVolume = 1;

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

    public static AudioManager instance { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
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

    private void Update()
    {
        masterBus.setVolume(masterVolume);
        musicBus.setVolume(musicVolume);
        ambienceBus.setVolume(ambienceVolume);
        sfxBus.setVolume(SFXVolume);
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
        Debug.Log("MUSICA---------------");
        musicEventInstance = CreateInstance(FMODEvents.instance.music);
        musicEventInstance.start();
    }

    public void SetAmbienceParameter(string parameterName, float parameterValue)
    {
        ambienceEventInstance.setParameterByName(parameterName, parameterValue);
    }
    
    public void SetInsideState(int state)
    {
        insideSnapshotEventInstance.setParameterByName("SnapshotState", (float) state);
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
