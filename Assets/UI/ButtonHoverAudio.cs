namespace Context.UI
{
    using UnityEngine;
    using UnityEngine.EventSystems;

    namespace Context.UI
    {
        public class ButtonHoverAudio : MonoBehaviour, IPointerEnterHandler
        {
            private AudioSource _audioSource;
            private AudioData _hoverData;

            public void Init(AudioSource audioSource, AudioData hoverData)
            {
                _audioSource = audioSource;
                _hoverData = hoverData;
            }

            public void OnPointerEnter(PointerEventData eventData)
            {
                if (_hoverData != null)
                    ApplicationManager.Instance.AudioManager.Play(_hoverData, _audioSource);
            }
        }
    }
}