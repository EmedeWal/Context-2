namespace Context.ThirdPersonController
{
    using UnityEngine;

    public class TPManager : MonoBehaviour
    {
        private TPController _controller;
        private TPAudio _audio;
        private TPRoot _root;

        private InputActions _inputActions;
        private Transform _cameraTransform;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;

            _controller = GetComponentInChildren<TPController>();
            _audio = GetComponentInChildren<TPAudio>();
            _root = GetComponentInChildren<TPRoot>();

            _inputActions = ApplicationManager.Instance.InputManager.Actions;
            _cameraTransform = Camera.main.transform;

            _controller.Init();
            _audio.Init();
            _root.Init(_controller.transform);

            gameObject.layer = Layers.GetControllerLayer();
        }

        private void OnDisable()
        {
            _root.Cleanup();
            _audio.Cleanup();
        }

        private void Update()
        {
            var inputActions = _inputActions.Gameplay;
            var controllerInput = new ControllerInput
            {
                Rotation = _cameraTransform.rotation,
                Movement = inputActions.Move.ReadValue<Vector2>(),
                Interact = inputActions.Interact.WasPressedThisFrame(),
                Jump = inputActions.Jump.WasPressedThisFrame(),
                CancelJump = inputActions.Jump.WasReleasedThisFrame(),
                SustainJump = inputActions.Jump.IsPressed(),
            };
            _controller.Tick(controllerInput);
        }

        private void LateUpdate()
        {
            var groundType = _controller.GetGroundType();

            _root.LateTick(groundType, Time.deltaTime, _controller.IsMoving());
            _audio.LateTick(groundType);
        }
    }
}