namespace Context
{
    using UnityEngine;
    using System;

    public class Timer
    {
        private readonly Action _completed;
        private float _remainingTime;
        private float _duration;

        public Timer(float duration, Action completed)
        {
            _remainingTime = duration;
            _completed = completed;
            _duration = duration;

            if (completed == null)
                Debug.LogError("Cannot create a timer with a null action!");
        }

        public virtual void Init(float duration)
        {
            _remainingTime = _duration = duration;
        }

        public Timer Cleanup()
        {
            return null;
        }

        public void Tick(float deltaTime)
        {
            _remainingTime -= deltaTime;

            if (_remainingTime <= 0)
                OnCompleted();
        }

        public void OnCompleted()
        {
            _remainingTime = 0;
            _completed();
        }

        public float GetInvertedProgress() =>
            Mathf.Clamp01(1f - (_remainingTime / _duration));

        public float GetProgress() =>
            Mathf.Clamp01(_remainingTime / _duration);
    }
}