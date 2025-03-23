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

        private const float _locomotionTransitionTime = 0.2f;
        private const float _actionTransitionTime = 0.1f;
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
            TPController.InteractionStarted += TPAnimator_InteractionStarted;
            TPController.InteractionCanceled += TPAnimator_InteractionCanceled;
        }

        public void Cleanup()
        {
            TPController.Jumped -= TPAnimator_Jumped;
            TPController.InteractionStarted -= TPAnimator_InteractionStarted;
            TPController.InteractionCanceled -= TPAnimator_InteractionCanceled;
        }

        public void UpdateAnimations(float deltaTime, bool isMoving)
        {
            _deltaTime = deltaTime;

            CrossFade(isMoving ? _walkH : _idleH, _deltaTime);
        }

        public void OnFootstep() =>
            Footstep?.Invoke();

        private void CrossFade(int hash, float deltaTime, int layer = _defaultLayer, float transitionTime = _locomotionTransitionTime)
        {
            var currentAnimation = _animator.GetCurrentAnimatorStateInfo(layer);
            var nextAnimation = _animator.GetNextAnimatorStateInfo(layer);

            if (hash != currentAnimation.shortNameHash && hash != nextAnimation.shortNameHash)
                _animator.CrossFade(hash, transitionTime, layer, deltaTime);
        }

        private void TPAnimator_Jumped()
        {
            CrossFade(_jumpH, _deltaTime, _overrideLayer, _actionTransitionTime);
        }

        private void TPAnimator_InteractionStarted()
        {
            CrossFade(_interactH, _deltaTime, _overrideLayer, _actionTransitionTime);
        }

        private void TPAnimator_InteractionCanceled()
        {
            CrossFade(_emptyH, _deltaTime, _overrideLayer, _actionTransitionTime);
        }
    }
}