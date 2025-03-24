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

        public float DefaultOpacity { get; private set; } = 0f; // Default brightness level (adjustable)

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
            SetOverlayAlpha(DefaultOpacity);
        }

        public void Tick(float deltaTime)
        {
            if (_fadeHaltTimer != null)
            {
                SetOverlayAlpha(1f);
                _fadeHaltTimer.Tick(deltaTime);
            }
            else if (_fadeInTimer != null)
            {
                var progress = _fadeInTimer.GetInvertedProgress();
                SetOverlayAlpha(Mathf.Lerp(DefaultOpacity, 1f, progress));
                _fadeInTimer.Tick(deltaTime);
            }
            else if (_fadeOutTimer != null)
            {
                var progress = _fadeOutTimer.GetInvertedProgress();
                SetOverlayAlpha(Mathf.Lerp(1f, DefaultOpacity, progress));
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
            SetOverlayAlpha(DefaultOpacity);
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
            SetOverlayAlpha(DefaultOpacity);

            _fadeCompleteAction?.Invoke();
            _fadeOutTimer = _fadeOutTimer?.Cleanup();
        }

        private void SetOverlayAlpha(float alpha)
        {
            var color = _overlay.color;
            color.a = Mathf.Clamp01(alpha);
            _overlay.color = color;
        }

        public void SetDefaultOpacity(float value)
        {
            DefaultOpacity = Mathf.Clamp01(value);
            SetOverlayAlpha(DefaultOpacity); // Immediately apply new default brightness
        }

        private bool IsInTransition() =>
            _fadeHaltTimer != null || _fadeOutTimer != null || _fadeInTimer != null;
    }
}