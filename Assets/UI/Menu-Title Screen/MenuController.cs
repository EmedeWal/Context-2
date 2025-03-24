namespace Context.UI
{
    using global::Context.UI.Context.UI;
    using UnityEngine.EventSystems;
    using System.Collections;
    using UnityEngine.UI;
    using UnityEngine;

    public class MenuScreenController : MonoBehaviour
    {
        [Header("REFERENCES")]

        [Space]
        [Header("Other")]
        [SerializeField] private GameObject _menuHolder;

        [Space]
        [Header("Buttons")]
        [SerializeField] private Button[] _buttons;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _quitToMenuButton;
        [SerializeField] private Button _quitGameButton;

        [Space]
        [Header("Sliders")]
        [SerializeField] private VolumeSetting[] _volumeSettings;
        [SerializeField] private BrightnessSetting _brightnessSetting;

        [Space]
        [Header("Audio Data")]
        [SerializeField] private AudioData _hoverData;
        [SerializeField] private AudioData _pauseData;
        [SerializeField] private AudioData _unpauseData;

        [Space]
        [Header("Audio Sources")]
        [SerializeField] private AudioSource _hoverSource;
        [SerializeField] private AudioSource _pauseSource;

        private InputActions _inputActions;
        private EventSystem _eventSystem;

        private void Start()
        {
            var firstSelected = _resumeButton.gameObject;

            _inputActions = ApplicationManager.Instance.InputManager.Actions;
            _eventSystem = EventSystem.current;

            if (ApplicationManager.Instance.InputManager.GetInputType() > 0)
            {
                _eventSystem.SetSelectedGameObject(firstSelected);
                _eventSystem.firstSelectedGameObject = firstSelected;
            }

            _resumeButton.onClick.AddListener(Resume);
            _quitToMenuButton.onClick.AddListener(QuitToMainMenu);
            _retryButton.onClick.AddListener(Retry);
            _quitGameButton.onClick.AddListener(QuitGame);

            foreach (var item in _volumeSettings)
                item.Init();
            _brightnessSetting.Init();

            // Attach hover sound script to each button
            foreach (var button in _buttons)
            {
                var hoverAudio = button.gameObject.AddComponent<ButtonHoverAudio>();
                hoverAudio.Init(_hoverSource, _hoverData);
            }

            SetMenuActive(false, false);
        }

        private void OnDisable()
        {
            _retryButton.onClick.RemoveAllListeners();
            _resumeButton.onClick.RemoveAllListeners();
            _quitToMenuButton.onClick.RemoveAllListeners();

            foreach (var item in _volumeSettings)
                item.Cleanup();
            _brightnessSetting.Cleanup();
        }

        private void Update()
        {
            var menu = _inputActions.Menu;
            if (menu.Pause.WasPressedThisFrame())
                ToggleMenu();

            if (ApplicationManager.Instance.InputManager.GetInputType() == 0) return;

            var currentSelected = _eventSystem.currentSelectedGameObject;
            if (currentSelected == null) _eventSystem.SetSelectedGameObject(_resumeButton.gameObject);
        }

        private void Resume()
        {
            SetMenuActive(false, true);
        }

        private void Retry()
        {
            Time.timeScale = 1.0f;

            var sceneLoader = ApplicationManager.Instance.SceneLoader;
            sceneLoader.TransitionToScene(sceneLoader.GetCurrentBuildIndex());
        }

        private void QuitToMainMenu()
        {
            Time.timeScale = 1.0f;

            ApplicationManager.Instance.SceneLoader.TransitionToScene(0);
        }

        private void QuitGame()
        {
            Time.timeScale = 1.0f;

            Application.Quit();
        }

        private void ToggleMenu()
        {
            SetMenuActive(!_menuHolder.activeSelf, true);
        }

        private void SetMenuActive(bool active, bool audio)
        {
            var activeKeyboard = active && ApplicationManager.Instance.InputManager.GetInputType() == 0;
            var selectedObject = activeKeyboard ? null : _resumeButton.gameObject;
            var audioData = active ? _pauseData : _unpauseData;

            _menuHolder.SetActive(active);
            Time.timeScale = active ? 0 : 1;

            Cursor.lockState = activeKeyboard
                ? CursorLockMode.None
                : CursorLockMode.Locked;
            _eventSystem.SetSelectedGameObject(selectedObject);
            if (audio) ApplicationManager.Instance.AudioManager.Play(audioData, _pauseSource); 

            StartCoroutine(ManagButtons());
        }

        private IEnumerator ManagButtons()
        {
            foreach (var button in _buttons)
                button.interactable = false;

            yield return new WaitForSecondsRealtime(0.5f);

            foreach (var button in _buttons)
                button.interactable = true;
        }
    }
}