namespace Context.ThirdPersonController
{
    using UnityEngine;

    public class TPRoot : MonoBehaviour
    {
        [Header("REFERENCES")]

        [Space]
        [Header("Particles")]
        [SerializeField] private ParticleSystem _smallDust;
        [SerializeField] private ParticleSystem _bigDust;

        private Transform _followTarget;
        private Transform _transform;
        private TPAnimator _animator;
        private GroundType _groundType;

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

        public void LateTick(GroundType groundType, float deltaTime, bool isMoving, bool isSprinting)
        {
            _transform.SetPositionAndRotation(_followTarget.position, _followTarget.rotation);

            _animator.UpdateAnimations(deltaTime, isMoving, isSprinting);

            _groundType = groundType;
        }

        private void TPRoot_Footstep()
        {
            if (_groundType is GroundType.Sand)
            {
                _smallDust.Play();
            }
        }

        private void TPRoot_Landed(GroundType groundType)
        {
            if (groundType is GroundType.Sand)
            {
                _bigDust.Play();
            }
        }
    }
}