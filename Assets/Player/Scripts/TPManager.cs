namespace Context.ThirdPersonController
{
    using UnityEngine;

    public class TPManager : MonoBehaviour
    {
        private TPController _controller;
        private TPAudio _audio;
        private TPRoot _root;

        private CutsceneManager _cutsceneManager;
        private InputActions _inputActions;
        private Transform _cameraTransform;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;

            _controller = GetComponentInChildren<TPController>();
            _audio = GetComponentInChildren<TPAudio>();
            _root = GetComponentInChildren<TPRoot>();

            _cutsceneManager = CutsceneManager.Instance;
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
            if (_cutsceneManager.IsPlayingCutscene())
                return;

            var inputActions = _inputActions.Gameplay;
            var controllerInput = new ControllerInput
            {
                Rotation = _cameraTransform.rotation,
                Movement = inputActions.Move.ReadValue<Vector2>(),
                Jump = inputActions.Jump.WasPressedThisFrame(),
                CancelJump = inputActions.Jump.WasReleasedThisFrame(),
                SustainJump = inputActions.Jump.IsPressed(),
                SustainSprint = inputActions.Sprint.IsPressed(),
                Interact = inputActions.Interact.WasPressedThisFrame(),
            };
            _controller.Tick(controllerInput);
        }

        private void LateUpdate()
        {
            if (_cutsceneManager.IsPlayingCutscene())
                return;

            var groundType = _controller.GetGroundType();

            _root.LateTick(groundType, Time.deltaTime, _controller.IsMoving, _controller.IsSprinting);
            _audio.LateTick(groundType);
        }
    }
}