namespace Context
{
    using System.Collections;
    using Unity.Mathematics;
    using UnityEngine;

    public class MovingObject : MonoBehaviour
    {
        [SerializeField] private Transform _moveTarget;
        [SerializeField] private float _speed;

        private Transform _transform;
        private Vector3 _startPosition;
        private Vector3 _targetPosition;

        private void Start()
        {
            _transform = transform;
            _startPosition = _transform.position;
            _targetPosition = _moveTarget.position;    
        }

        public void Activate()
        {
            StopAllCoroutines();
            StartCoroutine(MoveCoroutine(_targetPosition));
        }

        public void Deactivate()
        {
            StopAllCoroutines();
            StartCoroutine(MoveCoroutine(_startPosition));
        }

        private IEnumerator MoveCoroutine(Vector3 targetPosition)
        {
            while (Vector3.Distance(_transform.position, targetPosition) > 0.1f)
            {
                var response = 1f - Mathf.Exp(-_speed * Time.deltaTime);
                _transform.position = Vector3.Lerp(_transform.position, targetPosition, response);
                yield return null;
            }
        }
    }
}