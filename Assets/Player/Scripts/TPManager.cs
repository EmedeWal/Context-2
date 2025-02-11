namespace Context.Player
{
    using UnityEngine;

    public class TPManager : MonoBehaviour
    {
        private TPController _controller;
        private TPCamera _camera;
        private TPRoot _root;

        private InputActions _inputActions;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;

            _controller = GetComponentInChildren<TPController>();
            _camera = GetComponentInChildren<TPCamera>();
            _root = GetComponentInChildren<TPRoot>();

            _inputActions = new InputActions();
            _inputActions.Enable();

            var targetTransform = _controller.transform;
            _controller.Init();
            _camera.Init(targetTransform);
            _root.Init(targetTransform);

            gameObject.layer = Layers.GetControllerLayer();
        }

        private void OnDisable()
        {
            _inputActions.Dispose();
        }

        private void Update()
        {
            var inputActions = _inputActions.Gameplay;
            var deltaTime = Time.deltaTime;

            var lookInput = inputActions.Look.ReadValue<Vector2>();
            _camera.Tick(lookInput, deltaTime);

            var controllerInput = new ControllerInput
            {
                Rotation = _camera.Rotation,
                Movement = inputActions.Move.ReadValue<Vector2>(),
                Jump = inputActions.Jump.WasPressedThisFrame(),
                CancelJump = inputActions.Jump.WasReleasedThisFrame(),
                SustainJump = inputActions.Jump.IsPressed(),
            };
            _controller.Tick(controllerInput);
        }

        private void LateUpdate()
        {
            _camera.LateTick();
            _root.LateTick();
        }
    }
}