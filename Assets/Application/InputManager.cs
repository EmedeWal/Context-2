namespace Context
{
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;

    public class InputManager
    {
        public InputActions Actions { get; private set; }

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

        public InputManager()
        {
            InputSystem.onDeviceChange += OnDeviceChange;
            UpdateInputType(); // Initialize input type at start

            Actions = new();
            Actions.Enable();
        }

        public void Cleanup()
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