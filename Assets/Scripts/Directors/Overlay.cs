namespace Context
{
    using UnityEngine.UI;
    using UnityEngine;
    using System;

    public class Overlay
    {
        private readonly Image _overlay;

        private Action _fadeCompleteAction;
        private Action _fadeHaltAction;
        private Timer _fadeHaltTimer;
        private Timer _fadeOutTimer;
        private Timer _fadeInTimer;
        private float _fadeTime;

        public Overlay(GameObject canvasObject)
        {
            _overlay = canvasObject.GetComponentInChildren<Image>();

            _fadeCompleteAction = null;
            _fadeHaltAction = null;
            _fadeHaltTimer = null;
            _fadeOutTimer = null;
            _fadeInTimer = null;
            _fadeTime = 0;

            _overlay.color = Color.black;
            SetOverlayAlpha(0f);
        }

        public void Tick(float deltaTime)
        {
            if (_fadeHaltTimer != null)
            {
                _fadeHaltTimer.Tick(deltaTime);
            }
            else if (_fadeInTimer != null)
            {
                SetOverlayAlpha(_fadeInTimer.GetInvertedProgress());
                _fadeInTimer.Tick(deltaTime);
            }
            else if (_fadeOutTimer != null)
            {
                SetOverlayAlpha(_fadeOutTimer.GetProgress());
                _fadeOutTimer.Tick(deltaTime);
            }
        }

        public void FadeRoutineOverlay(Action fadeHaltAction, Action fadeCompleteAction, Color overlayColor, float fadeTime)
        {
            if (IsInTransition()) return;

            _fadeCompleteAction = fadeCompleteAction;
            _fadeHaltAction = fadeHaltAction;
            _overlay.color = overlayColor;
            _fadeTime = fadeTime;

            OnSceneTransitionStarted();
        }

        private void OnSceneTransitionStarted()
        {
            SetOverlayAlpha(0f);

            _fadeInTimer = new(_fadeTime, () => FadeInCompleted());
        }

        private void FadeInCompleted()
        {
            SetOverlayAlpha(1f);

            _fadeHaltAction?.Invoke();
            _fadeInTimer = _fadeInTimer?.Cleanup();
            _fadeHaltTimer = new(_fadeTime, () => StartFadeOut());
        }

        private void StartFadeOut()
        {
            _fadeHaltTimer = _fadeHaltTimer?.Cleanup();
            _fadeOutTimer = new(_fadeTime, () => OnSceneTransitionFinished());
        }

        private void OnSceneTransitionFinished()
        {
            SetOverlayAlpha(0f);

            _fadeCompleteAction?.Invoke();
            _fadeOutTimer = _fadeOutTimer?.Cleanup();
        }

        private void SetOverlayAlpha(float alpha)
        {
            var color = _overlay.color;
            color.a = Mathf.Clamp01(alpha);
            _overlay.color = color;
        }

        private bool IsInTransition() =>
            _fadeHaltTimer != null || _fadeOutTimer != null || _fadeInTimer != null;
    }
}