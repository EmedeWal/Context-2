namespace Context
{
    using UnityEngine;

    public class ParticleController : MonoBehaviour
    {
        private ParticleSystem.EmissionModule _emissionModule;
        private float _emissionCount;

        public void Init()
        {
            _emissionModule = GetComponent<ParticleSystem>().emission;
            _emissionCount = _emissionModule.rateOverTime.constant;
        }

        public void Tick(float completedPercentage)
        {
            var newRate = Mathf.Lerp(0, _emissionCount, completedPercentage);
            _emissionModule.rateOverTime = new ParticleSystem.MinMaxCurve(newRate);
        }
    }
}
