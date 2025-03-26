namespace Context
{
    using UnityEngine.EventSystems;
    using UnityEngine;
    using UnityEngine.Rendering;

    public class ApplicationManager : MonoBehaviour
    {
        public static ApplicationManager Instance { get; private set; }

        //

        public PostProcessingManager PostProcessingManager => _postProcessingManager;
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
        [Header("Volumes")]
        [SerializeField] private VolumeProfile _standardProfile;
        [SerializeField] private VolumeProfile _runtimeProfile;
        [SerializeField] private VolumeProfile _aliveProfile;
        [SerializeField] private VolumeProfile _deadProfile;

        private PostProcessingManager _postProcessingManager;
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
                _inputManager = new();
                _audioManager = new(GetComponentsInChildren<AudioSource>());
                _postProcessingManager = new(gameObject, _standardProfile, _runtimeProfile, _aliveProfile, _deadProfile);

                Instance = this;
            }
            else
                Destroy(gameObject);

            Instance.Init();
        }

        public void Init()
        {
            _postProcessingManager.Init();
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