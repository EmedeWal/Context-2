namespace Context.UI
{
    using UnityEngine.SceneManagement;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using UnityEngine;

    public class TitleScreenController : MonoBehaviour
    {
        [Header("REFERENCES")]

        [Space]
        [Header("VLG's")]
        [SerializeField] private GameObject _buttonVLG;
        [SerializeField] private GameObject _sliderVLG;

        [Space]
        [Header("Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private Button _creditsButton;
        [SerializeField] private Button _optionsButton;

        [Space]
        [Header("Selection")]
        [SerializeField] private GameObject _currentButton;
        [SerializeField] private GameObject _currentSlider;

        [Space]
        [Header("Sliders")]
        [SerializeField] private Slider[] _sliders;
        [SerializeField] private VolumeSetting[] _volumeSettings;
        [SerializeField] private BrightnessSetting _brightnessSetting;

        [Space]
        [Header("Audio")]
        [SerializeField] private AudioClip _onClick;
        [SerializeField] private AudioClip _onSlide;

        private InputActions _inputActions;
        private EventSystem _eventSystem;
        private AudioSource _audioSource;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            var firstSelected = _playButton.gameObject;

            _inputActions = InputManager.Instance.Actions;
            _eventSystem = EventSystem.current;
            _audioSource = GetComponent<AudioSource>();

            _eventSystem.SetSelectedGameObject(firstSelected);
            _eventSystem.firstSelectedGameObject = firstSelected;

            _playButton.onClick.AddListener(OnButtonClick);
            _quitButton.onClick.AddListener(OnButtonClick);
            _creditsButton.onClick.AddListener(OnButtonClick);
            _optionsButton.onClick.AddListener(OnButtonClick);

            _playButton.onClick.AddListener(Play);
            _quitButton.onClick.AddListener(Quit);
            _creditsButton.onClick.AddListener(Credits);
            _optionsButton.onClick.AddListener(Options);

            foreach (var item in _volumeSettings)
                item.Init();
            _brightnessSetting.Init();

            foreach (var item in _sliders)
                item.onValueChanged.AddListener(OnSliderChanged);

            _sliderVLG.SetActive(false);
        }

        private void OnDisable()
        {
            _playButton.onClick.RemoveAllListeners();
            _quitButton.onClick.RemoveAllListeners();
            _creditsButton.onClick.RemoveAllListeners();
            _optionsButton.onClick.RemoveAllListeners();

            foreach (var item in _volumeSettings)
                item.Cleanup();
            _brightnessSetting.Cleanup();

            foreach (var item in _sliders)
                item.onValueChanged.RemoveListener(OnSliderChanged);
        }

        private void Update()
        {
            var currentSelected = _eventSystem.currentSelectedGameObject;
            var sliderVLGOpen = _sliderVLG.activeSelf;

            if (currentSelected == null)
            {
                var selected = sliderVLGOpen 
                    ? _currentSlider
                    : _currentButton;
                _eventSystem.SetSelectedGameObject(selected);
            }
            else
            {
                if (sliderVLGOpen) _currentSlider = currentSelected;
                else _currentButton = currentSelected;
            }

            var menuActions = _inputActions.Menu;
            if (menuActions.Close.WasPressedThisFrame() && _sliderVLG.activeSelf)
            {
                OnButtonClick();
                ManageSliderState(_currentButton, false);
            }
        }

        private void OnButtonClick()
        {
            if (_onClick != null)
                _audioSource.Play();
        }

        private void OnSliderChanged(float value)
        {
            if (_onSlide != null)
                _audioSource.Play();
        }

        private void Play() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        private void Quit() => Application.Quit();
        private void Credits() => Debug.LogWarning("Credits not implemented.");
        private void Options() => ManageSliderState(_currentSlider, true);

        private void ManageSliderState(GameObject selected, bool active)
        {
            _sliderVLG.SetActive(active);
            _eventSystem.SetSelectedGameObject(selected);
        }
    }
}