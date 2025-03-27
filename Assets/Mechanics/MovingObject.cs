namespace Context
{
    using System.Collections;
    using UnityEngine;

    public class MovingObject : MonoBehaviour
    {
        [SerializeField] private Transform _moveTarget;
        [SerializeField] private float _moveDuration;

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
            StartCoroutine(MoveCoroutine(_targetPosition));
        }

        public void Deactivate()
        {
            StartCoroutine(MoveCoroutine(_startPosition));
        }

        private IEnumerator MoveCoroutine(Vector3 targetPosition)
        {
            var elapsedTime = 0f;

            while (elapsedTime < _moveDuration)
            {
                var progress = Mathf.Clamp01(elapsedTime / _moveDuration);
                _transform.position = Vector3.Lerp(_transform.position, targetPosition, progress);

                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}