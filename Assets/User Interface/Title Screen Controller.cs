namespace Context.UI
{
    using UnityEngine.SceneManagement;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using UnityEngine;

    public class TitleScreenController : MonoBehaviour
    {
        [Header("SETTINGS")]

        [Space]
        [Header("Input")]
        [SerializeField] private float _lockTime = 0.2f;

        [Header("REFERENCES")]

        [Space]
        [Header("VLG's")]
        [SerializeField] private GameObject _buttonVLG;
        [SerializeField] private GameObject _sliderVLG;

        [Space]
        [Header("Sliders")]
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _soundSlider;
        [SerializeField] private Slider _brightnessSlider;

        [Space]
        [Header("Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private Button _creditsButton;
        [SerializeField] private Button _optionsButton;

        private InputActions _inputActions;
        private EventSystem _eventSystem;

        private GameObject _lastButton;
        private GameObject _lastSlider;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            var firstSelected = _playButton.gameObject;

            _inputActions = new InputActions();
            _eventSystem = EventSystem.current;

            _inputActions.Enable();
            _eventSystem.SetSelectedGameObject(firstSelected);
            _eventSystem.firstSelectedGameObject = firstSelected;

            _lastButton = firstSelected;
            _lastSlider = _musicSlider.gameObject;

            _playButton.onClick.AddListener(LockInput);
            _quitButton.onClick.AddListener(LockInput);
            _creditsButton.onClick.AddListener(LockInput);
            _optionsButton.onClick.AddListener(LockInput);

            _playButton.onClick.AddListener(Play);
            _quitButton.onClick.AddListener(Quit);
            _creditsButton.onClick.AddListener(Credits);
            _optionsButton.onClick.AddListener(Options);

            _sliderVLG.SetActive(false);
        }

        private void OnDisable()
        {
            _inputActions.Dispose();

            _playButton.onClick.RemoveAllListeners();
            _quitButton.onClick.RemoveAllListeners();
            _creditsButton.onClick.RemoveAllListeners();
            _optionsButton.onClick.RemoveAllListeners();
        }

        private void Update()
        {
            var currentSelected = _eventSystem.currentSelectedGameObject;
            var sliderVLGOpen = _sliderVLG.activeSelf;

            if (currentSelected == null)
            {
                var selected = sliderVLGOpen 
                    ? _lastSlider
                    : _lastButton;
                _eventSystem.SetSelectedGameObject(selected);
            }
            else
            {
                if (sliderVLGOpen) _lastSlider = currentSelected;
                else _lastButton = currentSelected;
            }

            var menuActions = _inputActions.Menu;
            if (menuActions.Cancel.WasPressedThisFrame() && _sliderVLG.activeSelf)
                ManageSliderState(_lastButton, false);
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

        private void Play() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        private void Quit() => Application.Quit();
        private void Credits() => Debug.LogWarning("Credits not implemented.");
        private void Options() => ManageSliderState(_lastSlider, true);

        private void ManageSliderState(GameObject selected, bool active)
        {
            _sliderVLG.SetActive(active);
            _eventSystem.SetSelectedGameObject(selected);
        }
    }
}