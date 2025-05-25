using System;
using BULLSHIT;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace Managers
{
    public class LoadManager : MonoBehaviour
    {
        private static LoadManager _instance;

        public static LoadManager Instance => _instance;

        [SerializeField] private VideoPlayer _videoPlayer;
        [SerializeField] private int _managersToLoad;
        
        private int _currentManagersToLoaded;
        private bool _hasVideoEnded;

        private float _time;

        private float _maxTime = 0.3f;

        private EventInstance _cutsceneAmbient;

        private void Awake()
        {
            _instance = this;
            _videoPlayer.loopPointReached += OnVideoEnd;

            _cutsceneAmbient = AudioManager.Instance.CreateInstance(FMODEvents.instance.cutscene);
            _cutsceneAmbient.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
            _cutsceneAmbient.start();
        }

        private void Update()
        {
            if (!Input.GetKey(KeyCode.Space))
            {
                Debug.Log("Not Pressing");
                _time = 0;
                return;
            }

            _time += Time.deltaTime;
            
            Debug.Log("Pressing");

            if (_time < _maxTime)
            {
                return;
            }
            
            Debug.Log("Finish");
            
            FinishVideo();
        }

        public void ManagerLoaded()
        {
            _currentManagersToLoaded++;
            
            CheckConditions();
        }

        private void OnVideoEnd(VideoPlayer videoPlayer)
        {
            FinishVideo();
        }

        private void FinishVideo()
        {
            _hasVideoEnded = true;

            _cutsceneAmbient.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            
            CheckConditions();
        }

        private void CheckConditions()
        {
            if (_currentManagersToLoaded != _managersToLoad || (!_hasVideoEnded && _videoPlayer.targetTexture != null))
            {
                return;
            }
            
            LoadNextScene();
            
            Destroy(gameObject);
        }

        private void LoadNextScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            //SceneManager.LoadScene("DemoScene");
#if UNITY_EDITOR
            CleanConsole cleanConsole = new CleanConsole();
            cleanConsole = null;
#endif
        }

        private void OnDestroy()
        {
            _videoPlayer.loopPointReached -= OnVideoEnd;
        }
    }
}