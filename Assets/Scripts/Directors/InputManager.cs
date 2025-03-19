namespace Context
{
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;
    using UnityEngine;

    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }
        public InputActions Actions { get; private set; }

        [SerializeField] private EventSystem _eventSystem;
        private InputType _currentInputType;

        public enum InputType
        {
            None = 0,
            Xbox = 1,
            PlayStation = 2
        }

        public enum InputMap
        {
            None,
            Gameplay,
            Menu,
            All,
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                DestroyImmediate(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InputSystem.onDeviceChange += OnDeviceChange;
            UpdateInputType(); // Initialize input type at start

            Actions = new();
            Actions.Enable();

            Instantiate(_eventSystem, transform);
        }

        private void OnApplicationQuit()
        {
            Actions.Disable();
            Actions.Dispose();

            InputSystem.onDeviceChange -= OnDeviceChange;
        }

        public void LockGameplayInput()
        {
            Actions.Gameplay.Disable();
        }

        public void UnlockGameplayInput()
        {
            Actions.Gameplay.Enable();
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected ||
                change == InputDeviceChange.Removed)
                UpdateInputType();
        }

        private void UpdateInputType()
        {
            if (Gamepad.current != null)
            {
                _currentInputType = Gamepad.current.layout switch
                {
                    string s when s.Contains("DualShock") || s.Contains("DualSense") => InputType.PlayStation,
                    string s when s.Contains("Xbox") || s.Contains("XInput") => InputType.Xbox,
                    _ => InputType.None,
                };
            }
            else
                _currentInputType = InputType.None;
        }

        public InputType GetInputType() => _currentInputType;
    }
}