namespace Context.ThirdPersonController
{
    using UnityEngine;

    public class TPRoot : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _smallDust;
        [SerializeField] private ParticleSystem _bigDust;

        private Transform _followTarget;
        private Transform _transform;
        
        private TPAnimator _animator;

        public void Init(Transform followTarget)
        {
            _followTarget = followTarget;
            _transform = transform;

            _animator = GetComponentInChildren<TPAnimator>();

            _animator.Init();

            TPController.Landed += TPRoot_Landed;
            TPAnimator.Footstep += TPRoot_Footstep;
        }

        public void Cleanup()
        {
            _animator.Cleanup();

            TPController.Landed -= TPRoot_Landed;
            TPAnimator.Footstep -= TPRoot_Footstep;
        }

        public void LateTick(float deltaTime, bool isMoving)
        {
            _transform.SetPositionAndRotation(_followTarget.position, _followTarget.rotation);

            _animator.UpdateAnimations(deltaTime, isMoving);
        }

        private void TPRoot_Footstep()
        {
            _smallDust.Play();
        }

        private void TPRoot_Landed()
        {
            _bigDust.Play();
        }
    }
}