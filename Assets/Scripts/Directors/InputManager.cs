namespace Context
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }
        private InputType _currentInputType = InputType.None;

        public enum InputType
        {
            None = 0,
            Xbox = 1,
            PlayStation = 2
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InputSystem.onDeviceChange += OnDeviceChange;
            UpdateInputType(); // Initialize input type at start
        }

        private void OnDestroy()
        {
            InputSystem.onDeviceChange -= OnDeviceChange;
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected ||
                change == InputDeviceChange.Removed)
            {
                UpdateInputType();
            }
        }

        private void UpdateInputType()
        {
            if (Gamepad.current != null)
            {
                string layout = Gamepad.current.layout;

                if (layout.Contains("DualShock") || layout.Contains("DualSense"))
                {
                    _currentInputType = InputType.PlayStation;
                }
                else if (layout.Contains("Xbox") || layout.Contains("XInput"))
                {
                    _currentInputType = InputType.Xbox;
                }
                else
                {
                    _currentInputType = InputType.None; // Unknown gamepad
                }
            }
            else
            {
                _currentInputType = InputType.None; // Default to PC if no gamepad
            }
        }

        public InputType GetInputType() => _currentInputType;
    }
}