namespace Context.ThirdPersonController
{
    using KinematicCharacterController;
    using UnityEngine;

    public class TPController : BaseConnectionPoint, ICharacterController
    {
        private KinematicCharacterMotor _motor;
        private TriggerChannel _channel;
        private TPInput _input;

        [Header("MOVEMENT")]

        [Space]
        [Header("Grounded")]
        [SerializeField] private float _acceleration = 2f;
        [SerializeField] private float _deceleration = 10f;
        [SerializeField] private float _groundedSpeed = 10f;
        [SerializeField] private float _groundedRotation = 5f;

        [Space]
        [Header("Airborne")]
        [SerializeField] private float _airSpeed = 6f;
        [SerializeField] private float _airRotation = 2f;
        [SerializeField] private float _maxVerticalVelocity = 30f;
        [SerializeField] private float _airManoeuvrability = 40f;
        [SerializeField] private float _gravity = -60f;
        [SerializeField] private float _jumpSustainMultiplier = 0.4f;

        [Space]
        [Header("Jumping")]
        [SerializeField] private float _jumpBuffer = 0.1f;
        [SerializeField] private float _coyoteTime = 0.2f;
        [SerializeField] private float _jumpForce = 20f;

        [Header("CONNECTIONS")]
        [SerializeField] private LayerMask _connectableLayer;
        [SerializeField] private float _checkRange = 3;

        private OtherConnectionStruct _oldStruct;
        private float _timeSinceJumpRequest;
        private float _airborneTime;
        private bool _forcedUnground;

        public void Init()
        {
            _motor = GetComponent<KinematicCharacterMotor>();
            _motor.CharacterController = this;

            _channel = GetComponentInChildren<TriggerChannel>();
            _channel.Init(this, _checkRange);

            _input = new();

            _channel.ConnectionEnter += TPController_ConnectionEnter;
            _channel.ConnectionExit += TPController_ConnectionExit;
        }

        public override void Cleanup()
        {
            base.Cleanup();

            _channel.Cleanup();
        }

        public void Tick(ControllerInput controllerInput) => _input.UpdateInput(controllerInput);
        public void SetTransientPosition(Vector3 position) => _motor.SetPosition(position);
        public Vector3 GetTransientPosition() => _motor.TransientPosition;

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            if (_motor.GroundingStatus.IsStableOnGround)
            {
                HandleGroundedLocomotion(ref currentVelocity, deltaTime);
            }
            else
            {
                HandleAirborneLocomotion(ref currentVelocity, deltaTime);
            }

            CheckJump(ref currentVelocity, deltaTime);

            CheckTransfer();

            var horizontalSpeed = Vector3.ProjectOnPlane(currentVelocity, _motor.CharacterUp).magnitude;
            var verticalSpeed = currentVelocity.y;
            BuildLogger.Instance.LogSpeed(horizontalSpeed, verticalSpeed);
        }

        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            if (_input.RequestedMovement.sqrMagnitude > 0.001f) // Only update rotation if moving
            {
                var rotationSpeed = _motor.GroundingStatus.IsStableOnGround
                    ? _groundedRotation
                    : _airRotation;

                var forward = Vector3.ProjectOnPlane
                (
                    _input.RequestedRotation * Vector3.forward,
                    _motor.CharacterUp
                );
                var targetRotation = Quaternion.LookRotation(forward, _motor.CharacterUp);

                if (targetRotation != _motor.TransientRotation)
                {
                    var response = 1f - Mathf.Exp(-rotationSpeed * deltaTime);
                    currentRotation = Quaternion.Slerp(currentRotation, targetRotation, response);
                }
            }
        }

        #region MOVEMENT
        #region Grounded
        private void HandleGroundedLocomotion(ref Vector3 currentVelocity, float deltaTime)
        {
            var groundedMovement = _motor.GetDirectionTangentToSurface
            (
                direction: _motor.CharacterForward,
                surfaceNormal: _motor.GroundingStatus.GroundNormal
            ) * _input.RequestedMovement.magnitude;

            var targetVelocity = groundedMovement * _groundedSpeed;
            var response = currentVelocity.magnitude < targetVelocity.magnitude
                ? _acceleration
                : _deceleration;

            var moveVelocity = Vector3.Lerp
            (
                a: currentVelocity,
                b: targetVelocity,
                t: 1f - Mathf.Exp(-response * deltaTime)
            );
            currentVelocity = moveVelocity;
        }
        #endregion

        #region Airborne
        private void HandleAirborneLocomotion(ref Vector3 currentVelocity, float deltaTime)
        {
            var characterUp = _motor.CharacterUp;
            var requestedMovement = _input.RequestedMovement;

            var planarMovement = Vector3.ProjectOnPlane
            (
                vector: requestedMovement,
                planeNormal: characterUp
            ) * requestedMovement.magnitude;

            var planarVelocity = Vector3.ProjectOnPlane(currentVelocity, characterUp);
            var movementForce = _airManoeuvrability * deltaTime * planarMovement;

            CalculateInputForce(ref movementForce, planarVelocity, planarMovement);
            CalculateGravity(ref currentVelocity, characterUp, deltaTime);

            currentVelocity += movementForce;

            ClampVertical(ref currentVelocity);
        }

        private void CalculateInputForce(ref Vector3 movementForce, Vector3 planarVelocity, Vector3 planarMovement)
        {
            var dot = Vector3.Dot(planarVelocity.normalized, planarMovement);

            // Only accelerate an airborne player while he is under the maximum _velocity magnitude
            if (planarVelocity.magnitude < _airSpeed)
            {
                var targetPlanarVelocity = planarVelocity + movementForce;
                targetPlanarVelocity = Vector3.ClampMagnitude(targetPlanarVelocity, _airSpeed);
                movementForce = targetPlanarVelocity - planarVelocity;
            }
            // Otherwise, limit the movement force in the same direction
            else if (dot > 0)
                movementForce = Vector3.ProjectOnPlane(movementForce, planarVelocity.normalized);
        }

        private void ClampVertical(ref Vector3 currentVelocity)
        {
            if (Mathf.Abs(currentVelocity.y) > _maxVerticalVelocity)
            {
                var clampedVertical = Mathf.Clamp(currentVelocity.y, -_maxVerticalVelocity, _maxVerticalVelocity);
                currentVelocity = new(currentVelocity.x, clampedVertical, currentVelocity.z);
            }
        }

        private void CalculateGravity(ref Vector3 currentVelocity, Vector3 characterUp, float deltaTime)
        {
            var sustainedJump = currentVelocity.y > 0 && _input.RequestedJumpSustain && _input.RequestedJumpCancel == false;
            var gravity = _gravity;

            if (sustainedJump)
                gravity *= _jumpSustainMultiplier;

            currentVelocity += gravity * deltaTime * characterUp;
        }
        #endregion

        #region Jumping
        private void CheckJump(ref Vector3 currentVelocity, float deltaTime)
        {
            UpdateState(deltaTime);

            if (_input.RequestedJump && _airborneTime < _coyoteTime && _forcedUnground == false)
                Jump(ref currentVelocity);
        }

        private void UpdateState(float deltaTime)
        {
            if (_motor.GroundingStatus.IsStableOnGround)
            {
                _input.RequestedJumpCancel = false;
                _forcedUnground = false;
                _airborneTime = 0;
            }
            else
                _airborneTime += deltaTime;

            // Remember jump input for the jumpBuffer time
            _timeSinceJumpRequest = _input.RequestedJump == false
                ? _timeSinceJumpRequest - deltaTime
                : _jumpBuffer;
        }

        private void Jump(ref Vector3 currentVelocity)
        {
            _motor.ForceUnground();
            _forcedUnground = true;
            _input.RequestedJump = false;

            var characterUp = _motor.CharacterUp;
            var currentVerticalSpeed = Vector3.Dot(currentVelocity, characterUp);
            var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, _jumpForce);

            currentVelocity += characterUp * (targetVerticalSpeed - currentVerticalSpeed);
        }
        #endregion
        #endregion

        private void CheckTransfer()
        {
            if (_input.RequestedTransfer)
            {
                var pos = _motor.TransientPosition;
                var hits = Physics.OverlapSphere(pos, _checkRange, _connectableLayer);

                if (hits.Length > 0)
                {
                    if (hits[0].TryGetComponent<BaseConnectionPoint>(out var component)) _manager.StabilizeConnections(component);
                    else Debug.LogWarning("Hit on connectable Layer had no base connection point script.");
                }
            }

            _input.RequestedTransfer = false;
        }

        private void TPController_ConnectionEnter(BaseConnectionPoint point)
        {
            _oldStruct = GetFirstOtherConnection();
            _manager.CreateUnstableConnection(this, point, _oldStruct);
        }

        private void TPController_ConnectionExit(BaseConnectionPoint point)
        {
            var newStruct = GetFirstOtherConnection();
            _manager.RemoveUnstableConnection(this, point, _oldStruct, newStruct);
        }

        #region Unused
        public void BeforeCharacterUpdate(float deltaTime) { }
        public void AfterCharacterUpdate(float deltaTime) { }
        public void PostGroundingUpdate(float deltaTime) { }
        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
        public bool IsColliderValidForCollisions(Collider collider) => true;
        public void OnDiscreteCollisionDetected(Collider hitCollider) { }
        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }
        #endregion
    }
}