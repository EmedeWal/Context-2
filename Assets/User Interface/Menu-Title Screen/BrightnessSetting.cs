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

        private const string BrightnessKey = "BrightnessValue"; // Key for saving brightness

        public void Init()
        {
            _slider = GetComponent<Slider>();

            // Set slider range
            _slider.minValue = -2f;  // Darker
            _slider.maxValue = 0f;   // Brighter

            // Ensure we get the Color Adjustments override
            if (_globalVolume.profile.TryGet(out _colorAdjustments))
            {
                float savedBrightness = PlayerPrefs.GetFloat(BrightnessKey, 0f); // Default to 0 (neutral)
                _colorAdjustments.postExposure.Override(savedBrightness);
                _slider.value = savedBrightness;
            }

            _slider.onValueChanged.AddListener(SetBrightness);
        }

        public void Cleanup()
        {
            _slider.onValueChanged.RemoveListener(SetBrightness);
        }

        private void SetBrightness(float value)
        {
            if (_colorAdjustments != null)
            {
                _colorAdjustments.postExposure.Override(value);
                PlayerPrefs.SetFloat(BrightnessKey, value); // Save brightness
                PlayerPrefs.Save(); // Ensure it's written to disk
            }
        }
    }
}