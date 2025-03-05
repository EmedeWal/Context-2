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
        private const string VolumeKey = "Volume_"; // Prefix for saving multiple volume settings

        public void Init()
        {
            _slider = GetComponent<Slider>();

            _slider.minValue = -80f;
            _slider.maxValue = 0f;

            string key = VolumeKey + _parameter; // Unique key for each volume parameter
            float savedVolume = PlayerPrefs.GetFloat(key, 0f); // Default to 0 dB (full volume)

            _mixer.SetFloat(_parameter, savedVolume);
            _slider.value = savedVolume;

            _slider.onValueChanged.AddListener(SetVolume);
        }

        public void Cleanup()
        {
            _slider.onValueChanged.RemoveListener(SetVolume);
        }

        private void SetVolume(float decibels)
        {
            _mixer.SetFloat(_parameter, decibels);
            PlayerPrefs.SetFloat(VolumeKey + _parameter, decibels); // Save the volume
            PlayerPrefs.Save(); // Ensure it's written to disk
        }
    }
}