namespace Context
{
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    public class PostProcessingManager
    {
        private readonly VolumeProfile _standardProfile;
        private readonly VolumeProfile _runtimeProfile;
        private readonly VolumeProfile _aliveProfile;
        private readonly VolumeProfile _deadProfile;
        private readonly Volume _volume;

        private const float SPEED = 5f; // Speed multiplier for smooth lerp

        public PostProcessingManager(GameObject parentObject, VolumeProfile standardProfile, VolumeProfile runtimeProfile, VolumeProfile aliveProfile, VolumeProfile deadProfile)
        {
            _volume = parentObject.GetComponentInChildren<Volume>();

            _standardProfile = standardProfile;
            _runtimeProfile = runtimeProfile;
            _aliveProfile = aliveProfile;
            _deadProfile = deadProfile;
        }

        public void Init()
        {
            _volume.profile = _standardProfile;
        }

        public void UpdateVolumeSettings(float completedPercentage, float deltaTime)
        {
            _volume.profile = _runtimeProfile;
            var response = 1f - Mathf.Exp(-SPEED * deltaTime);

            // Lerp Vignette settings
            if (_deadProfile.TryGet<Vignette>(out var deadVignette)
             && _aliveProfile.TryGet<Vignette>(out var aliveVignette)
             && _runtimeProfile.TryGet(out Vignette vignette))
            {
                var targetColor = Color.Lerp(deadVignette.color.value, aliveVignette.color.value, completedPercentage);
                vignette.color.Override(Color.Lerp(vignette.color.value, targetColor, response));
            }

            // Lerp Color Adjustments settings
            if (_deadProfile.TryGet<ColorAdjustments>(out var deadColor)
             && _aliveProfile.TryGet<ColorAdjustments>(out var aliveColor)
             && _runtimeProfile.TryGet<ColorAdjustments>(out var colorAdjustments))
            {
                var targetPostExposure = Mathf.Lerp(deadColor.postExposure.value, aliveColor.postExposure.value, completedPercentage);
                var targetContrast = Mathf.Lerp(deadColor.contrast.value, aliveColor.contrast.value, completedPercentage);
                var targetHueShift = Mathf.Lerp(deadColor.hueShift.value, aliveColor.hueShift.value, completedPercentage);
                var targetSaturation = Mathf.Lerp(deadColor.saturation.value, aliveColor.saturation.value, completedPercentage);
                var targetColorFilter = Color.Lerp(deadColor.colorFilter.value, aliveColor.colorFilter.value, completedPercentage);

                colorAdjustments.postExposure.Override(Mathf.Lerp(colorAdjustments.postExposure.value, targetPostExposure, response));
                colorAdjustments.contrast.Override(Mathf.Lerp(colorAdjustments.contrast.value, targetContrast, response));
                colorAdjustments.hueShift.Override(Mathf.Lerp(colorAdjustments.hueShift.value, targetHueShift, response));
                colorAdjustments.saturation.Override(Mathf.Lerp(colorAdjustments.saturation.value, targetSaturation, response));
                colorAdjustments.colorFilter.Override(Color.Lerp(colorAdjustments.colorFilter.value, targetColorFilter, response));
            }

            // Lerp White Balance settings
            if (_deadProfile.TryGet<WhiteBalance>(out var deadBalance)
             && _aliveProfile.TryGet<WhiteBalance>(out var aliveBalance)
             && _runtimeProfile.TryGet<WhiteBalance>(out var currentBalance))
            {
                var targetTemperature = Mathf.Lerp(deadBalance.temperature.value, aliveBalance.temperature.value, completedPercentage);
                var targetTint = Mathf.Lerp(deadBalance.tint.value, aliveBalance.tint.value, completedPercentage);

                currentBalance.temperature.Override(Mathf.Lerp(currentBalance.temperature.value, targetTemperature, response));
                currentBalance.tint.Override(Mathf.Lerp(currentBalance.tint.value, targetTint, response));
            }
        }
    }
}