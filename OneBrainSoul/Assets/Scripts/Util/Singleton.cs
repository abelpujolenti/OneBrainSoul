using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // _instance may disappear on scene change
    private static T _instance;

    // True if GameObject should be persistent between scenes
    // if false, it will be destroyed on scene change and it will rely on new scene's GameController Object
    [SerializeField]
    protected bool _persistent = true;

    public static T Instance
    {
        get
        {
            if (_instance != null) return _instance;

            Debug.Log($"[Singleton<{typeof(T)}>] An instance is needed in the scene and no existing instances were found, so a new instance will be created.");

            return _instance = new GameObject($"(Singleton){typeof(T)}").AddComponent<T>();
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null)
        {
            Debug.Log($"[Singleton<{typeof(T)}>] There should never be more than one Singleton of type {typeof(T)} in the scene. This redundant instance will be destroyed.");
            Destroy(gameObject);
            return;
        }

        _instance = this as T;

        if (_persistent) DontDestroyOnLoad(gameObject);

        OnAwake();
    }

    protected virtual void OnAwake() { }
}