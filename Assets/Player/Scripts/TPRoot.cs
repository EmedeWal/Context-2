namespace Context.ThirdPersonController
{
    using UnityEngine;

    public class TPRoot : MonoBehaviour
    {
        private Transform _followTarget;
        private Transform _transform;
        
        private TPAnimator _animator;

        public void Init(Transform followTarget)
        {
            _followTarget = followTarget;
            _transform = transform;

            _animator = GetComponentInChildren<TPAnimator>();

            _animator.Init();
        }

        public void Cleanup()
        {
            _animator.Cleanup();
        }

        public void LateTick(float deltaTime, bool isMoving)
        {
            _transform.SetPositionAndRotation(_followTarget.position, _followTarget.rotation);

            _animator.UpdateAnimations(deltaTime, isMoving);
        }
    }
}