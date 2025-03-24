namespace Context.UI
{
    using global::Context.UI.Context.UI;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using UnityEngine;

    public class TitleScreenController : MonoBehaviour
    {
        [Header("REFERENCES")]

        [Space]
        [Header("Holders")]
        [SerializeField] private GameObject _controlHolder; // Added this

        [Space]
        [Header("Buttons")]
        [SerializeField] private Button[] _buttons;
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
        [SerializeField] private AudioData _hoverData;

        private EventSystem _eventSystem;
        private AudioSource _audioSource;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            var firstSelected = _playButton.gameObject;

            _eventSystem = EventSystem.current;
            _audioSource = GetComponent<AudioSource>();

            var keyboard = ApplicationManager.Instance.InputManager.GetInputType() == 0;
            _controlHolder.SetActive(!keyboard); // Added this logic

            if (!keyboard)
            {
                _eventSystem.SetSelectedGameObject(firstSelected);
                _eventSystem.firstSelectedGameObject = firstSelected;
            }

            // Attach hover sound script to each button
            foreach (var button in _buttons)
            {
                var hoverAudio = button.gameObject.AddComponent<ButtonHoverAudio>();
                hoverAudio.Init(_audioSource, _hoverData);
            }

            _playButton.onClick.AddListener(Play);
            _quitButton.onClick.AddListener(Quit);
            _creditsButton.onClick.AddListener(Credits);

            foreach (var item in _volumeSettings)
                item.Init();
            _brightnessSetting.Init();
        }

        private void OnDisable()
        {
            _playButton.onClick.RemoveAllListeners();
            _quitButton.onClick.RemoveAllListeners();
            _creditsButton.onClick.RemoveAllListeners();

            foreach (var item in _volumeSettings)
                item.Cleanup();
            _brightnessSetting.Cleanup();
        }

        private void Update()
        {
            if (ApplicationManager.Instance.InputManager.GetInputType() == 0) return;

            var currentSelected = _eventSystem.currentSelectedGameObject;
            if (currentSelected == null) _eventSystem.SetSelectedGameObject(_playButton.gameObject);
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