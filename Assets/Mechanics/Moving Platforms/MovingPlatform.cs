namespace Context
{
    using System.Collections.Generic;
    using System.Collections;
    using UnityEngine;

    [RequireComponent(typeof(Collider))]    
    [RequireComponent(typeof(Rigidbody))]
    public class MovingPlatform : MonoBehaviour
    {
        [Header("ACTIVATION SETTINGS")]
        [field: SerializeField] public ActivationMode ActivationMode { get; private set; } = ActivationMode.Automatic;
        [SerializeField] private float _activationDelay = 2f;

        [Header("PATH SETTINGS")]
        [SerializeField] private float _journeyPause = 1f;
        [SerializeField] private float _waypointPause = 1f;
        [SerializeField] private float _movementSpeed = 4f;
        [SerializeField] private bool _allowIndepence = false;

        private List<Transform> _waypointList;
        private Rigidbody _rigidbody;
        private int _waypointIndex = 0;
        private bool _movingForward = true;

        private PassengerTrigger _trigger;
        private Transform _transform;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            _rigidbody.isKinematic = true;
            _rigidbody.useGravity = false;

            _trigger = GetComponentInChildren<PassengerTrigger>();
            _trigger.Init(this);
            _transform = transform;

            CollectWaypoints();

            if (ActivationMode is ActivationMode.Automatic)
                Activate();
        }

        private void Update()
        {
            if (HaltDueToIndepence())
            {
                _waypointIndex = 0;
                _movingForward = false;
            }
        }

        public void SetCollision()
        {
            ActivationMode = ActivationMode.Collision;
        }

        public void Activate()
        {
            ActivationMode = ActivationMode.None;
            _rigidbody.isKinematic = true;
            StartCoroutine(MovePlatform());
        }


        private IEnumerator MovePlatform()
        {
            yield return new WaitForSeconds(_activationDelay);

            while (true)
            {
                Transform targetWaypoint = _waypointList[_waypointIndex];

                while (Vector3.Distance(_transform.position, targetWaypoint.position) > 0.05f)
                {
                    targetWaypoint = _waypointList[_waypointIndex];

                    Vector3 newPosition = Vector3.MoveTowards(_transform.position, targetWaypoint.position, _movementSpeed * Time.deltaTime);
                    Vector3 movement = newPosition - _transform.position;
                    _transform.position = newPosition;

                    if (_trigger.Passenger != null)
                    {
                        var positon = _trigger.Passenger.GetTransientPosition();
                        _trigger.Passenger.SetTransientPosition(positon + movement);
                    }
                    yield return new WaitForEndOfFrame();
                }

                _transform.position = targetWaypoint.position;

                if (HaltDueToIndepence())
                    continue;

                if (_movingForward)
                {
                    _waypointIndex++;
                    if (_waypointIndex >= _waypointList.Count)
                    {
                        _movingForward = false;
                        _waypointIndex = _waypointList.Count - 1;
                        yield return new WaitForSeconds(_journeyPause);
                    }
                    else
                    {
                        yield return new WaitForSeconds(_waypointPause);
                    }
                }
                else
                {
                    _waypointIndex--;
                    if (_waypointIndex < 0)
                    {
                        _waypointIndex = 0;
                        _movingForward = true;
                        yield return new WaitForSeconds(_journeyPause);
                    }
                    else
                    {
                        yield return new WaitForSeconds(_waypointPause);
                    }
                }
            }
        }

        private void CollectWaypoints()
        {
            _waypointList = new List<Transform>();
            var parent = transform.parent;
            for (int i = 1; i < parent.childCount; i++)
                _waypointList.Add(parent.GetChild(i));

            if (_waypointList.Count < 2)
                Debug.LogWarning("MovingPlatform requires at least two waypoints.");
        }

        private bool HaltDueToIndepence() =>
            !_allowIndepence && _trigger.Passenger == null && _waypointIndex > 0 && _movingForward;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan; // Changed to cyan for better visibility
            CollectWaypoints();

            for (int i = 0; i < _waypointList.Count - 1; i++)
                if (_waypointList[i] != null && _waypointList[i + 1] != null)
                    Gizmos.DrawLine(_waypointList[i].position, _waypointList[i + 1].position);
        }
    }

    public enum ActivationMode
    {
        None,
        Collision,
        Automatic,
    }
}