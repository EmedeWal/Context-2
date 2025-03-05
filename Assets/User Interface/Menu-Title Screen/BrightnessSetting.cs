namespace Context.UI
{
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    public class BrightnessSetting : MonoBehaviour
    {
        [SerializeField] private Volume _globalVolume;  // Reference to the Global Volume
        private ColorAdjustments _colorAdjustments;
        private Slider _slider;

        public void Init()
        {
            _slider = GetComponent<Slider>();

            // Set slider range
            _slider.minValue = -2f;  // Darker
            _slider.maxValue = 0f;   // Brighter

            if (_globalVolume.profile.TryGet(out _colorAdjustments))
                _slider.value = _colorAdjustments.postExposure.value;

            _slider.onValueChanged.AddListener(SetBrightness);
        }

        public void Cleanup()
        {
            _slider.onValueChanged.RemoveListener(SetBrightness);
        }

        private void SetBrightness(float value)
        {
            if (_colorAdjustments != null)
                _colorAdjustments.postExposure.value = value;
        }
    }
}