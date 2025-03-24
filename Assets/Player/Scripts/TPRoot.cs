namespace Context.ThirdPersonController
{
    using UnityEngine;

    public class TPRoot : MonoBehaviour
    {
        private ParticleSystem _dustSystem;
        private Transform _followTarget;
        private Transform _transform;
        
        private TPAnimator _animator;

        public void Init(Transform followTarget)
        {
            _followTarget = followTarget;
            _transform = transform;

            _dustSystem = GetComponentInChildren<ParticleSystem>();
            _animator = GetComponentInChildren<TPAnimator>();

            _animator.Init();

            TPAnimator.Footstep += TPRoot_Footstep;
        }

        public void Cleanup()
        {
            _animator.Cleanup();

            TPAnimator.Footstep -= TPRoot_Footstep;
        }

        public void LateTick(float deltaTime, bool isMoving)
        {
            _transform.SetPositionAndRotation(_followTarget.position, _followTarget.rotation);

            _animator.UpdateAnimations(deltaTime, isMoving);
        }

        private void TPRoot_Footstep()
        {
            _dustSystem.Play();
        }
    }
}