namespace Context.ThirdPersonController
{
    using UnityEngine;

    public class TPCamera : MonoBehaviour
    {
        public Quaternion Rotation => _transform.rotation;

        [Header("SETTINGS")]
        [SerializeField] private Vector3 _offset = new(0, 1.5f, 7.5f);
        [SerializeField] private float _sensitivity = 0.5f;
        [SerializeField] private bool _invertControls = false;
        [SerializeField] private float _maximumAngle = 30f;
        [SerializeField] private float _mimimumAngle = -15f;

        [Header("Smoothing")]
        [SerializeField] private float _smoothTimeX = 0.05f;
        [SerializeField] private float _smoothTimeY = 0.2f;
        [SerializeField] private float _lookSmoothing = 0.2f;

        private Transform _cameraTransform;
        private Transform _pivotTransform;
        private Transform _followTarget;
        private Transform _transform;

        private float _lookAngle;
        private float _tiltAngle;
        private float _smoothVelocityX;
        private float _smoothVelocityY;
        private float _smoothX;
        private float _smoothY;

        private Vector3 _currentVelocity; // Used for SmoothDamp

        public void Init(Transform followTarget, Camera mainCamera)
        {
            _cameraTransform = mainCamera.transform;
            _followTarget = followTarget;

            _pivotTransform = transform.GetChild(0);
            _transform = transform;

            _transform.SetPositionAndRotation(_followTarget.position, _followTarget.rotation);
            _lookAngle = _transform.rotation.eulerAngles.y;
        }

        public void Tick(Vector2 input)
        {
            var sensitivity = _sensitivity * (_invertControls ? -1 : 1);
            var horizontal = input.x * sensitivity;
            var vertical = input.y * sensitivity;

            if (_lookSmoothing > 0)
            {
                _smoothX = Mathf.SmoothDamp(_smoothX, horizontal, ref _smoothVelocityX, _lookSmoothing);
                _smoothY = Mathf.SmoothDamp(_smoothY, vertical, ref _smoothVelocityY, _lookSmoothing);
            }
            else
            {
                _smoothX = horizontal;
                _smoothY = vertical;
            }

            _tiltAngle -= _smoothY;
            _pivotTransform.localRotation = Quaternion.Euler(_tiltAngle, 0, 0);
            _tiltAngle = Mathf.Clamp(_tiltAngle, _mimimumAngle, _maximumAngle);

            _lookAngle += _smoothX;
            _transform.rotation = Quaternion.Euler(0, _lookAngle, 0);
        }

        public void LateTick(float deltaTime)
        {
            UpdatePosition();
            UpdateCamera();
        }

        private void UpdatePosition()
        {
            if (!_followTarget) return;

            // Separate horizontal (X, Z) and vertical (Y) smoothing
            float smoothX = Mathf.SmoothDamp(_transform.position.x, _followTarget.position.x, ref _currentVelocity.x, _smoothTimeX);
            float smoothY = Mathf.SmoothDamp(_transform.position.y, _followTarget.position.y, ref _currentVelocity.y, _smoothTimeY);
            float smoothZ = Mathf.SmoothDamp(_transform.position.z, _followTarget.position.z, ref _currentVelocity.z, _smoothTimeX);

            _transform.position = new Vector3(smoothX, smoothY, smoothZ);
        }

        private void UpdateCamera()
        {
            _cameraTransform.localPosition = _offset;
            _cameraTransform.LookAt(_transform.position);
        }
    }
}
