namespace Context.Player
{
    using UnityEngine;

    public class TPCamera : MonoBehaviour
    {
        public Quaternion Rotation => _transform.rotation;

        [Header("SETTINGS")]
        [SerializeField] private float _sensitivity = 2f;
        [SerializeField] private float _distance = 4f;
        [SerializeField] private float _height = 1.5f;

        private const float _verticalClamping = 75f;
        private const float _lookDamping = 0.15f;

        private Transform _followTarget;
        private Transform _transform;
        private Vector2 _smoothedLookInput;
        private Vector2 _lookInputVelocity;
        private Vector3 _eulerAngles;

        public void Init(Transform followTarget)
        {
            _followTarget = followTarget;
            _transform = transform;

            _transform.eulerAngles = _eulerAngles = _followTarget.eulerAngles;
        }

        public void Tick(Vector2 input, float deltaTime)
        {
            var targetLook = new Vector2(-input.y * _sensitivity, input.x * _sensitivity);
            _smoothedLookInput = Vector2.SmoothDamp
            (
                current: _smoothedLookInput,
                target: targetLook,
                currentVelocity: ref _lookInputVelocity,
                smoothTime: _lookDamping,
                maxSpeed: float.PositiveInfinity,
                deltaTime: deltaTime
            );
            _eulerAngles += new Vector3(_smoothedLookInput.x, _smoothedLookInput.y);

            _eulerAngles.x = Mathf.Clamp(_eulerAngles.x, -_verticalClamping, _verticalClamping);
        }

        public void LateTick() => UpdatePosition();

        private void UpdatePosition()
        {
            // Calculate the desired camera position
            Quaternion rotation = Quaternion.Euler(_eulerAngles);
            Vector3 offset = rotation * new Vector3(0, _height, -_distance);
            _transform.position = _followTarget.position + offset;

            // Always look at the player
            _transform.LookAt(_followTarget.position + Vector3.up * _height);
        }
    }
}