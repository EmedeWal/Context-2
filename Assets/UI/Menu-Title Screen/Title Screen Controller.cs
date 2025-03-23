namespace Context.UI
{
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using UnityEngine;

    public class TitleScreenController : MonoBehaviour
    {
        [Header("REFERENCES")]

        [Space]
        [Header("Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private Button _creditsButton;

        [Space]
        [Header("Sliders")]
        [SerializeField] private Slider[] _sliders;
        [SerializeField] private VolumeSetting[] _volumeSettings;
        [SerializeField] private BrightnessSetting _brightnessSetting;

        [Space]
        [Header("Audio")]
        [SerializeField] private AudioData _clickData;
        [SerializeField] private AudioData _slideData;

        private EventSystem _eventSystem;
        private AudioSource _audioSource;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            var firstSelected = _playButton.gameObject;

            _eventSystem = EventSystem.current;
            _audioSource = GetComponent<AudioSource>();

            if (ApplicationManager.Instance.InputManager.GetInputType() > 0)
            {
                _eventSystem.SetSelectedGameObject(firstSelected);
                _eventSystem.firstSelectedGameObject = firstSelected;
            }

            _playButton.onClick.AddListener(OnButtonClick);
            _quitButton.onClick.AddListener(OnButtonClick);
            _creditsButton.onClick.AddListener(OnButtonClick);

            _playButton.onClick.AddListener(Play);
            _quitButton.onClick.AddListener(Quit);
            _creditsButton.onClick.AddListener(Credits);

            foreach (var item in _volumeSettings)
                item.Init();
            _brightnessSetting.Init();

            foreach (var item in _sliders)
                item.onValueChanged.AddListener(OnSliderChanged);
        }

        private void OnDisable()
        {
            _playButton.onClick.RemoveAllListeners();
            _quitButton.onClick.RemoveAllListeners();
            _creditsButton.onClick.RemoveAllListeners();

            foreach (var item in _volumeSettings)
                item.Cleanup();
            _brightnessSetting.Cleanup();

            foreach (var item in _sliders)
                item.onValueChanged.RemoveListener(OnSliderChanged);
        }

        private void Update()
        {
            if (ApplicationManager.Instance.InputManager.GetInputType() == 0) return;

            var currentSelected = _eventSystem.currentSelectedGameObject;
            if (currentSelected == null) _eventSystem.SetSelectedGameObject(_playButton.gameObject);
        }

        private void OnButtonClick()
        {
            if (_clickData != null)
                ApplicationManager.Instance.AudioManager.Play(_clickData, _audioSource);
        }

        private void OnSliderChanged(float value)
        {
            if (_slideData != null)
                ApplicationManager.Instance.AudioManager.Play(_slideData, _audioSource);
        }

        private void Play()
        {
            var nextIndex = ApplicationManager.Instance.SceneLoader.GetCurrentBuildIndex() + 1;
            ApplicationManager.Instance.SceneLoader.TransitionToScene(nextIndex);
        }

        private void Quit() => Application.Quit();
        private void Credits() => Debug.LogWarning("Credits not implemented.");
    }
}