namespace Context
{
    using System.Collections;
    using UnityEngine;

    public class MovingObject : MonoBehaviour
    {
        [SerializeField] private Transform _moveTarget;
        [SerializeField] private float _moveDuration;

        private Transform _transform;
        private Vector3 _targetPosition;

        private void Start()
        {
            _transform = transform;
            _targetPosition = _moveTarget.position;    
        }

        public void Activate()
        {
            StartCoroutine(MoveCoroutine());
        }

        private IEnumerator MoveCoroutine()
        {
            var elapsedTime = 0f;

            while (elapsedTime < _moveDuration)
            {
                var progress = Mathf.Clamp01(elapsedTime / _moveDuration);
                _transform.position = Vector3.Lerp(_transform.position, _targetPosition, progress);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            Destroy(_moveTarget.gameObject);
            Destroy(this);
        }
    }
}