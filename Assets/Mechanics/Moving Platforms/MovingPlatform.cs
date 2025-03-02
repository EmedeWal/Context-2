namespace Context
{
    using System.Collections.Generic;
    using System.Collections;
    using UnityEngine;

    [RequireComponent(typeof(Rigidbody))]
    public class MovingPlatform : MonoBehaviour
    {
        [Header("ACTIVATION SETTINGS")]
        [SerializeField] private float _activationDelay = 2f;

        [Header("PATH SETTINGS")]
        [SerializeField] private float _movementSpeed = 4f;
        [SerializeField] private float _waypointPause = 1f;
        [SerializeField] private float _journeyPause = 1f;

        private List<Transform> _waypointList;
        private Rigidbody _rigidbody;
        private int _waypointIndex = 0;
        private bool _movingForward = true;

        private void Start()
        {
            var rb = GetComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.isKinematic = false;
            rb.useGravity = false;
            _rigidbody = rb;

            _waypointList = new List<Transform>();
            var parent = transform.parent;
            for (int i = 1; i < parent.childCount; i++) 
                _waypointList.Add(parent.GetChild(i));

            if (_waypointList.Count < 2)
            {
                Debug.LogError("MovingPlatform requires at least two waypoints.");
                return;
            }

            StartCoroutine(MovePlatform());
        }

        private IEnumerator MovePlatform()
        {
            yield return new WaitForSeconds(_activationDelay);

            while (true)
            {
                Transform targetWaypoint = _waypointList[_waypointIndex];

                while (Vector3.Distance(transform.position, targetWaypoint.position) > 0.1f)
                {
                    Vector3 direction = (targetWaypoint.position - transform.position).normalized;
                    _rigidbody.linearVelocity = _movementSpeed * direction;
                    yield return new WaitForFixedUpdate();
                }

                _rigidbody.linearVelocity = Vector3.zero;
                transform.position = targetWaypoint.position;

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

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;

            _waypointList = new List<Transform>();
            var parent = transform.parent;
            for (int i = 1; i < parent.childCount; i++)
                _waypointList.Add(parent.GetChild(i));

            if (_waypointList != null && _waypointList.Count > 1)
            {
                for (int i = 0; i < _waypointList.Count - 1; i++)
                {
                    if (_waypointList[i] != null && _waypointList[i + 1] != null)
                    {
                        Gizmos.DrawLine(_waypointList[i].position, _waypointList[i + 1].position);
                    }
                }
            }
        }
    }
}
