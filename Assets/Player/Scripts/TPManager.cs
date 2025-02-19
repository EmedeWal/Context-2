namespace Context.ThirdPersonController
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
            _controller = GetComponentInChildren<TPController>();
            _camera = GetComponentInChildren<TPCamera>();
            _root = GetComponentInChildren<TPRoot>();

            _inputActions = new InputActions();
            _inputActions.Enable();

            var mainCamera = Camera.main;
            if (mainCamera == null)
                Debug.LogError("Forgot to assign a camera with the 'main' tag!");

            var controllerTransform = _controller.transform;
            var cameraTarget = _controller.transform.GetChild(0);

            _controller.Init();
            _camera.Init(cameraTarget, mainCamera);
            _root.Init(controllerTransform);

            gameObject.layer = Layers.GetControllerLayer();
        }

        private void OnDisable()
        {
            _inputActions.Dispose();
        }

        private void Update()
        {
            var inputActions = _inputActions.Gameplay;

            var lookInput = inputActions.Look.ReadValue<Vector2>();
            _camera.Tick(lookInput);

            var controllerInput = new ControllerInput
            {
                Rotation = _camera.Rotation,
                Movement = inputActions.Move.ReadValue<Vector2>(),
                Transfer = inputActions.Transfer.WasPressedThisFrame(),
                Jump = inputActions.Jump.WasPressedThisFrame(),
                CancelJump = inputActions.Jump.WasReleasedThisFrame(),
                SustainJump = inputActions.Jump.IsPressed(),
            };
            _controller.Tick(controllerInput);
        }

        private void LateUpdate()
        {
            var deltaTime = Time.deltaTime;

            _camera.LateTick(deltaTime);
            _root.LateTick();
        }
    }
}