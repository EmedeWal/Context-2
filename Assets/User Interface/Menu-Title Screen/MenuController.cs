namespace Context.UI
{
    using UnityEngine.SceneManagement;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using UnityEngine;

    public class MenuController : MonoBehaviour
    {
        [Header("SETTINGS")]
        [Space]
        [Header("Input")]
        [SerializeField] private float _lockTime = 0.2f;

        [Header("REFERENCES")]
        [Space]
        [SerializeField] private GameObject _menuHolder;
        [SerializeField] private GameObject _buttonVLG;
        [SerializeField] private GameObject _sliderVLG;

        [Space]
        [Header("Buttons")]
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _quitToMainButton;
        [SerializeField] private Button _quitGameButton;
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
            _inputActions = new InputActions();
            _eventSystem = EventSystem.current;
            _audioSource = GetComponent<AudioSource>();

            _inputActions.Enable();

            _resumeButton.onClick.AddListener(OnButtonClick);
            _retryButton.onClick.AddListener(OnButtonClick);
            _quitToMainButton.onClick.AddListener(OnButtonClick);
            _quitGameButton.onClick.AddListener(OnButtonClick);
            _optionsButton.onClick.AddListener(OnButtonClick);

            _resumeButton.onClick.AddListener(Resume);
            _retryButton.onClick.AddListener(Retry);
            _quitToMainButton.onClick.AddListener(QuitToMainMenu);
            _quitGameButton.onClick.AddListener(QuitGame);
            _optionsButton.onClick.AddListener(Options);

            foreach (var item in _volumeSettings)
                item.Init();
            _brightnessSetting.Init();

            foreach (var item in _sliders)
                item.onValueChanged.AddListener(OnSliderChanged);

            _menuHolder.SetActive(false);
            _sliderVLG.SetActive(false);
        }

        private void OnDisable()
        {
            _inputActions.Dispose();

            _resumeButton.onClick.RemoveAllListeners();
            _retryButton.onClick.RemoveAllListeners();
            _quitToMainButton.onClick.RemoveAllListeners();
            _quitGameButton.onClick.RemoveAllListeners();
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
                var selected = sliderVLGOpen ? _currentSlider : _currentButton;
                _eventSystem.SetSelectedGameObject(selected);
            }
            else
            {
                if (sliderVLGOpen) _currentSlider = currentSelected;
                else _currentButton = currentSelected;
            }

            var menuActions = _inputActions.Menu;
            if (menuActions.Cancel.WasPressedThisFrame() && _sliderVLG.activeSelf)
            {
                OnButtonClick();
                ManageSliderState(_currentButton, false);
            }
            else if (menuActions.Toggle.WasPressedThisFrame())
                ToggleMenu();
        }

        private void OnButtonClick()
        {
            if (_onClick != null)
                _audioSource.Play();

            LockInput();
        }

        private void OnSliderChanged(float value)
        {
            if (_onSlide != null)
                _audioSource.Play();
        }

        private void LockInput()
        {
            _inputActions.Disable();
            _eventSystem.currentInputModule.enabled = false;

            CancelInvoke();
            Invoke(nameof(UnlockInput), _lockTime);
        }

        private void UnlockInput()
        {
            _inputActions.Enable();
            _eventSystem.currentInputModule.enabled = true;
        }

        private void Resume()
        {
            SetMenuActive(false);
        }

        private void Retry()
        {
            SetMenuActive(false);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void QuitToMainMenu()
        {
            SetMenuActive(false);
            SceneManager.LoadScene(0);
        }

        private void QuitGame()
        {
            Application.Quit();
        }

        private void Options()
        {
            ManageSliderState(_currentSlider, true);
        }

        private void ManageSliderState(GameObject selected, bool active)
        {
            _sliderVLG.SetActive(active);
            _eventSystem.SetSelectedGameObject(selected);
        }

        private void ToggleMenu()
        {
            SetMenuActive(!_menuHolder.activeSelf);
        }

        private void SetMenuActive(bool active)
        {
            _menuHolder.SetActive(active);
            Time.timeScale = active ? 0 : 1;

            if (active)
            {
                _eventSystem.SetSelectedGameObject(_currentButton);
            }
            else
            {
                _sliderVLG.SetActive(false);
            }
        }
    }
}
