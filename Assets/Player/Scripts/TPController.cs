namespace Context.ThirdPersonController
{
    using KinematicCharacterController;
    using UnityEngine;
    using System;
    using System.Runtime.InteropServices.WindowsRuntime;

    public class TPController : BaseConnectionPoint, ICharacterController
    {
        public bool IsMoving => Vector3.ProjectOnPlane(_motor.Velocity, _motor.CharacterUp).sqrMagnitude > 0.1f;
        public bool IsSprinting => _input.RequestedSustainedSprint;

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
        [SerializeField] private float _sprintMultiplier = 2f;

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

        [Header("INTERACTION")]

        [Space]
        [Header("Connection")]
        [SerializeField] private LayerMask _interactionLayer;
        [SerializeField] private float _interactionDuration = 1f;
        [SerializeField] private float _interactionRange = 3;

        [Space]
        [Header("Detection")]
        [SerializeField] private int _sandLayer = 0;
        [SerializeField] private float _minFallVelocity = 10f;
        [SerializeField] private float _detectionDistance = 1f;

        public static event Action Add;
        public static event Action Remove;
        public static event Action Jumped;
        public static event Action<GroundType> Landed;

        private Timer _interactSustainTimer;
        private Vector3 _temporaryVelocity;
        private Vector3 _currentVelocity;
        private Vector3 _previousVelocity;
        private float _timeSinceJumpRequest;
        private float _airborneTime;
        private bool _initializedGrounded;
        private bool _previouslyGrounded;
        private bool _currentlyGrounded;
        private bool _forcedUnground;

        public void Init()
        {
            _motor = GetComponent<KinematicCharacterMotor>();
            _motor.CharacterController = this;

            _channel = GetComponentInChildren<TriggerChannel>();
            _channel.Init(this, _interactionRange);

            _input = new();

            _interactSustainTimer = null;
            _timeSinceJumpRequest = 0;
            _airborneTime = 0;
            _initializedGrounded = false;
            _previouslyGrounded = true;
            _currentlyGrounded = true;
            _forcedUnground = false;

            _channel.ConnectionEnter += TPController_ConnectionEnter;
            _channel.ConnectionExit += TPController_ConnectionExit;
        }

        public override void Cleanup()
        {
            base.Cleanup();

            _channel.Cleanup();
        }

        public override bool HasMaxConnections(Connection exception) => false;

        public GroundType GetGroundType()
        {
            if (Physics.Raycast(_motor.TransientPosition, -_motor.CharacterUp, out var hit, _detectionDistance))
            {
                return (int)hit.transform.gameObject.layer == _sandLayer
                    ? GroundType.Sand
                    : GroundType.Rock;
            }
            return GroundType.Rock;
        }

        public void Tick(ControllerInput controllerInput) => _input.UpdateInput(controllerInput);
        public void SetTransientPosition(Vector3 position) => _motor.SetPosition(position);
        public Vector3 GetTransientPosition() => _motor.TransientPosition;

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            if (_interactSustainTimer != null)
            {
                _interactSustainTimer.Tick(deltaTime);
                _input.RequestedInteract = false;
                currentVelocity = Vector3.zero;
                return;
            }

            if (_currentlyGrounded) HandleGroundedLocomotion(ref currentVelocity, deltaTime);
            else HandleAirborneLocomotion(ref currentVelocity, deltaTime);

            CheckJump(ref currentVelocity, deltaTime);
            CheckConnect();
        }

        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            // Only update lookRotation if moving and not interacting
            if (_input.RequestedMovement.sqrMagnitude > 0.001f && _interactSustainTimer == null)
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
            var responseFactor = currentVelocity.magnitude < targetVelocity.magnitude
                ? _acceleration
                : _deceleration;

            if (_input.RequestedSustainedSprint)
            {
                targetVelocity *= _sprintMultiplier;
                responseFactor *= _sprintMultiplier;
            }

            var moveVelocity = Vector3.Lerp
            (
                a: currentVelocity,
                b: targetVelocity,
                t: 1f - Mathf.Exp(-responseFactor * deltaTime)
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

            if (_input.RequestedJump && _airborneTime < _coyoteTime && _forcedUnground == false && _previouslyGrounded)
                OnJumped(ref currentVelocity);
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

        private void OnJumped(ref Vector3 currentVelocity)
        {
            Jumped?.Invoke();

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

        private void CheckConnect()
        {
            if (_input.RequestedInteract && _motor.GroundingStatus.IsStableOnGround && WorldSpaceCanvas.Instance.IsEnabled)
            {
                var pos = _motor.TransientPosition;
                var hits = Physics.OverlapSphere(pos, _interactionRange, _interactionLayer);

                if (hits.Length == 0) return;

                if (hits[0].TryGetComponent<StaticConnectionPoint>(out var component))
                {
                    if (_manager.HasStableConnection(this, component)) OnRemove();
                    else OnAdd();

                    component.StartConnection(pos);
                    var direction = component.transform.position - pos;
                    direction = Vector3.ProjectOnPlane(direction, _motor.CharacterUp);
                    _motor.SetRotation(Quaternion.LookRotation(direction, _motor.CharacterUp));

                    _interactSustainTimer = new
                    (
                        duration: _interactionDuration,
                        completed: () =>
                        {
                            _manager.InteractWithConnections(this, component);
                            _interactSustainTimer = _interactSustainTimer.Cleanup();
                        }
                    );
                }
                else Debug.LogWarning("Hit on connectable Layer had no base connection point script.");

            }
            _input.RequestedInteract = false;
        }

        private void OnAdd() =>
            Add?.Invoke();

        private void OnRemove() =>
            Remove?.Invoke();

        private void OnLanded(GroundType groundType) =>
            Landed?.Invoke(groundType);

        private void TPController_ConnectionEnter(BaseConnectionPoint point) =>
            _manager.CreateUnstableConnection(this, point);

        private void TPController_ConnectionExit(BaseConnectionPoint point) =>
            _manager.RemoveUnstableConnection(this, point);

        public void BeforeCharacterUpdate(float deltaTime)
        {
            _temporaryVelocity = _currentVelocity;
        }
        public void AfterCharacterUpdate(float deltaTime)
        {
            _previouslyGrounded = _currentlyGrounded;
            _currentlyGrounded = _motor.GroundingStatus.IsStableOnGround;
            _initializedGrounded = (_currentlyGrounded && _previouslyGrounded)|| _initializedGrounded;

            _currentVelocity = _motor.Velocity;
            _previousVelocity = _temporaryVelocity;
        }

        public void PostGroundingUpdate(float deltaTime) { }
        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            if (!_previouslyGrounded && !_currentlyGrounded && _initializedGrounded && Mathf.Abs(_previousVelocity.y) > _minFallVelocity)
                OnLanded(GetGroundType());
        }
        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
        public bool IsColliderValidForCollisions(Collider collider) => true;
        public void OnDiscreteCollisionDetected(Collider hitCollider) { }
        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }
    }
}