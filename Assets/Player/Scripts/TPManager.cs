namespace Context.ThirdPersonController
{
    using UnityEngine;

    public class TPManager : MonoBehaviour
    {
        private TPController _controller;
        private TPRoot _root;

        private InputActions _inputActions;
        private Transform _cameraTransform;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;

            _controller = GetComponentInChildren<TPController>();
            _root = GetComponentInChildren<TPRoot>();

            _inputActions = new InputActions();
            _inputActions.Enable();
            _cameraTransform = Camera.main.transform;

            _controller.Init();
            _root.Init(_controller.transform);

            gameObject.layer = Layers.GetControllerLayer();
        }

        private void OnDisable()
        {
            _inputActions.Disable();
            _inputActions.Dispose();
        }

        private void Update()
        {
            var inputActions = _inputActions.Gameplay;
            var controllerInput = new ControllerInput
            {
                Rotation = _cameraTransform.rotation,
                Movement = inputActions.Move.ReadValue<Vector2>(),
                Connect = inputActions.Connect.WasPressedThisFrame(),
                Jump = inputActions.Jump.WasPressedThisFrame(),
                CancelJump = inputActions.Jump.WasReleasedThisFrame(),
                SustainJump = inputActions.Jump.IsPressed(),
            };
            _controller.Tick(controllerInput);
        }

        private void LateUpdate()
        {
            _root.LateTick();
        }
    }
}