namespace Context.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class BrightnessSetting : MonoBehaviour
    {
        private Overlay _overlayScript;
        private Slider _slider;

        private float _highBrightness = 0f;
        private float _lowBrightness = 0.5f;  

        private const string BrightnessKey = "BrightnessValue"; // Save key

        public void Init()
        {
            _overlayScript = ApplicationManager.Instance.Overlay;
            _slider = GetComponent<Slider>();

            // Set brightness range (0 = fully bright, 0.2 = slightly darker)
            _slider.minValue = 0f;  // Fully transparent (brightest)
            _slider.maxValue = 1f;  // Slightly dark

            // Load saved brightness value
            float savedBrightness = PlayerPrefs.GetFloat(BrightnessKey, 0f);
            _slider.value = savedBrightness;

            _overlayScript.SetDefaultOpacity(TranslateValue(savedBrightness));

            _slider.onValueChanged.AddListener(SetBrightness);
        }

        public void Cleanup()
        {
            _slider.onValueChanged.RemoveListener(SetBrightness);
        }

        private void SetBrightness(float value)
        {
            _overlayScript.SetDefaultOpacity(TranslateValue(value));
            PlayerPrefs.SetFloat(BrightnessKey, value);
            PlayerPrefs.Save();
        }

        private float TranslateValue(float value) =>
            Mathf.Lerp(_lowBrightness, _highBrightness, value);
    }
}