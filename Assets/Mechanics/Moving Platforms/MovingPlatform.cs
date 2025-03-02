namespace Context
{
    using Context.ThirdPersonController;
    using System.Collections.Generic;
    using System.Collections;
    using UnityEngine;

    [RequireComponent(typeof(Collider))]    
    [RequireComponent(typeof(Rigidbody))]
    public class MovingPlatform : MonoBehaviour
    {
        [Header("ACTIVATION SETTINGS")]
        [SerializeField] private ActivationMode _activationMode = ActivationMode.Automatic;
        [SerializeField] private float _activationDelay = 2f;

        [Header("PATH SETTINGS")]
        [SerializeField] private float _movementSpeed = 4f;
        [SerializeField] private float _waypointPause = 1f;
        [SerializeField] private float _journeyPause = 1f;

        private List<Transform> _waypointList;
        private Rigidbody _rigidbody;
        private int _waypointIndex = 0;
        private bool _movingForward = true;

        private TPController _passenger;
        private Transform _transform;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            _rigidbody.isKinematic = true;
            _rigidbody.useGravity = false;

            _transform = transform;

            CollectWaypoints();

            if (_activationMode == ActivationMode.Automatic)
                Activate();
        }

        public void Activate()
        {
            _activationMode = ActivationMode.None;
            _rigidbody.isKinematic = true;
            StartCoroutine(MovePlatform());
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.TryGetComponent(out TPController controller))
            {
                if (_activationMode is ActivationMode.Collision)
                    Activate();

                _passenger = controller;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.transform.TryGetComponent(out TPController controller))
            {
                _passenger = null;
            }
        }

        private IEnumerator MovePlatform()
        {
            yield return new WaitForSeconds(_activationDelay);

            while (true)
            {
                Transform targetWaypoint = _waypointList[_waypointIndex];

                while (Vector3.Distance(_transform.position, targetWaypoint.position) > 0.05f)
                {
                    Vector3 newPosition = Vector3.MoveTowards(_transform.position, targetWaypoint.position, _movementSpeed * Time.deltaTime);
                    Vector3 movement = newPosition - _transform.position;
                    _transform.position = newPosition;

                    if (_passenger != null)
                    {
                        var positon = _passenger.GetTransientPosition();
                        _passenger.SetTransientPosition(positon + movement);
                    }

                    yield return null;
                }

                _transform.position = targetWaypoint.position;

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