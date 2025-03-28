namespace Context
{
    using System.Collections;
    using UnityEngine;

    public class Organism : MonoBehaviour
    {
        [Header("REFERENCES")]

        [Space]
        [Header("Hierarchy")]
        [SerializeField] private MeshRenderer[] _meshRenderers;

        [Space]
        [Header("Colors")]
        [SerializeField] private Color _aliveColor;
        [SerializeField] private Color _deadColor;

        private Color _targetColor;
        private Coroutine _colorCoroutine;

        public void SetState(bool alive, float duration)
        {
            _targetColor = alive ? _aliveColor : _deadColor;

            // Stop any ongoing color transition
            if (_colorCoroutine != null)
                StopCoroutine(_colorCoroutine);

            // Start new transition
            _colorCoroutine = StartCoroutine(ColorCoroutine(duration));
        }

        public void SetDeadColor(float time)
        {
            _targetColor = _deadColor;
            StartCoroutine(ColorCoroutine(time));
        }

        private IEnumerator ColorCoroutine(float duration)
        {
            float elapsedTime = 0f;
            Color[] startColors = new Color[_meshRenderers.Length];

            // Store the initial colors of all mesh renderers
            for (int i = 0; i < _meshRenderers.Length; i++)
                startColors[i] = _meshRenderers[i].material.color;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;

                for (int i = 0; i < _meshRenderers.Length; i++)
                    _meshRenderers[i].material.color = Color.Lerp(startColors[i], _targetColor, t);

                yield return null;
            }

            // Ensure the final color is set
            for (int i = 0; i < _meshRenderers.Length; i++)
                _meshRenderers[i].material.color = _targetColor;
        }
    }
}