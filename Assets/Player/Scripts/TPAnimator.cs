namespace Context.ThirdPersonController
{
    using UnityEngine;
    using System;

    [RequireComponent(typeof(Animator))]
    public class TPAnimator : MonoBehaviour
    {
        private Animator _animator;
        private int _interactH;
        private int _jumpH;
        private int _walkH;
        private int _idleH;
        private int _emptyH;

        [Header("SETTINGS")]

        [Space]
        [Header("Smoothing")]
        [SerializeField] private float _locomotionTransitionTime = 0.2f;
        [SerializeField] private float _actionTransitionTime = 0.1f;

        private const int _overrideLayer = 1;
        private const int _defaultLayer = 0;

        private float _deltaTime;

        public static event Action Footstep;

        public void Init()
        {
            _animator = GetComponent<Animator>();

            _interactH = Animator.StringToHash("Interact");
            _jumpH = Animator.StringToHash("Jump");
            _walkH = Animator.StringToHash("Walk");
            _idleH = Animator.StringToHash("Idle");
            _emptyH = Animator.StringToHash("Empty");

            TPController.Jumped += TPAnimator_Jumped;
            TPController.Landed += TPAnimator_Landed;
            TPController.Remove += TPAnimator_InteractionStarted;
            TPController.Add += TPAnimator_InteractionStarted;
        }

        public void Cleanup()
        {
            TPController.Jumped -= TPAnimator_Jumped;
            TPController.Landed -= TPAnimator_Landed;
            TPController.Remove -= TPAnimator_InteractionStarted;
            TPController.Add -= TPAnimator_InteractionStarted;
        }

        public void UpdateAnimations(float deltaTime, bool isMoving)
        {
            _deltaTime = deltaTime;

            CrossFade(isMoving ? _walkH : _idleH, _deltaTime, _locomotionTransitionTime);
        }

        public void OnFootstep()
        {
            var stateInfo = _animator.GetCurrentAnimatorStateInfo(_overrideLayer);
            if (stateInfo.shortNameHash != _emptyH) return;

            Footstep?.Invoke();
        }


        private void CrossFade(int hash, float deltaTime, float transitionTime, bool loopProtection = true, int layer = _defaultLayer)
        {
            var currentAnimation = _animator.GetCurrentAnimatorStateInfo(layer);
            var nextAnimation = _animator.GetNextAnimatorStateInfo(layer);

            if (loopProtection && (hash == currentAnimation.shortNameHash || hash == nextAnimation.shortNameHash))
                return;

            _animator.CrossFade(hash, transitionTime, layer, deltaTime);
        }

        private void TPAnimator_Jumped()
        {
            CrossFade(_jumpH, _deltaTime, _actionTransitionTime, false, _overrideLayer);
        }

        private void TPAnimator_Landed(Vector3 velocity)
        {
            CrossFade(_emptyH, _deltaTime, _locomotionTransitionTime, false, _overrideLayer);
        }

        private void TPAnimator_InteractionStarted()
        {
            CrossFade(_interactH, _deltaTime, _actionTransitionTime, false, _overrideLayer);
        }
    }
}