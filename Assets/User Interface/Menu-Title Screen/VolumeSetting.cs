namespace Context.UI
{
    using UnityEngine.Audio;
    using UnityEngine.UI;
    using UnityEngine;

    public class VolumeSetting : MonoBehaviour
    {
        [Space]
        [Header("Mixer")]
        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private string _parameter;

        private Slider _slider;

        public void Init()
        {
            _slider = GetComponent<Slider>();

            _slider.minValue = -80f;
            _slider.maxValue = 0;
            
            _mixer.GetFloat(_parameter, out var value);
            _slider.value = value;

            _slider.onValueChanged.AddListener(SetVolume);
        }

        public void Cleanup()
        {
            _slider.onValueChanged.RemoveListener(SetVolume);
        }

        private void SetVolume(float decibels) => _mixer.SetFloat(_parameter, decibels);
    }
}