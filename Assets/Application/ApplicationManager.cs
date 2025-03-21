namespace Context
{
    using System.Collections.Generic;
    using UnityEngine.InputSystem.UI;
    using UnityEngine.EventSystems;
    using UnityEngine.Audio;
    using UnityEngine;

    public class ApplicationManager : MonoBehaviour
    {
        public static ApplicationManager Instance { get; private set; }

        //

        public InputManager InputManager => _inputManager;
        public AudioManager AudioManager => _audioManager;
        public SceneLoader SceneLoader => _sceneLoader;
        public Overlay Overlay => _overlay;

        // 

        [Header("REFERENCES")]

        [Space]
        [Header("Prefabs")]
        [SerializeField] private GameObject _eventSystem;
        [SerializeField] private GameObject _canvas;

        [Space]
        [Header("Audio")]
        [SerializeField] private AudioData[] _audioTracks;
        [SerializeField] private AudioSource _audioSource;

        private InputManager _inputManager;
        private AudioManager _audioManager;
        private SceneLoader _sceneLoader;
        private Overlay _overlay;

        private void Start()
        {
            if (Instance == null)
            {
                DontDestroyOnLoad(gameObject);

                var canvas = Instantiate(_canvas, transform);
                var eventSytem = Instantiate(_eventSystem, transform);

                EventSystem.current = eventSytem.GetComponent<EventSystem>();
                Application.runInBackground = false;
                Application.targetFrameRate = 60;

                _overlay = new(canvas);
                _sceneLoader = new(_overlay);
                _audioManager = new(_audioSource, _audioTracks);
                _inputManager = new();

                Instance = this;
            }
            else
                Destroy(gameObject);
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;

            _audioManager.Tick();
            _sceneLoader.Tick(deltaTime);
        }

        private void OnApplicationQuit()
        {
            if (Instance == this)
            {
                _inputManager.Cleanup();
            }
        }
    }
}