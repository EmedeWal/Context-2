namespace Context.ThirdPersonController
{
    using UnityEngine;

    public class TPAudio : MonoBehaviour
    {
        [Header("REFERENCES")]

        [Space]
        [Header("Sources")]
        [SerializeField] private AudioSource _footstepSource;
        [SerializeField] private AudioSource _interactSource;
        [SerializeField] private AudioSource _jumpSource;
        [SerializeField] private AudioSource _landSource;

        [Space]
        [Header("Data")]
        [SerializeField] private AudioData[] _sandFootstepsArray;
        [SerializeField] private AudioData[] _rockFootstepsArray;
        [SerializeField] private AudioData _removeData;
        [SerializeField] private AudioData _addData;
        [SerializeField] private AudioData _jumpData;
        [SerializeField] private AudioData _landData;

        [Header("SETTINGS")]

        [Space]
        [Header("Landing")]
        [SerializeField] private float _minImpactMagnitude = 10;

        private AudioManager _audioManager;
        private GroundType _groundType;

        public void Init()
        {
            _audioManager = ApplicationManager.Instance.AudioManager;

            TPController.Add += TPAudio_Add;
            TPController.Remove += TPAudio_Remove;
            TPController.Jumped += TPAudio_Jumped;
            TPController.Landed += TPAudio_Landed;
            TPAnimator.Footstep += TPAudio_Footstep;
        }

        public void LateTick(GroundType groundType)
        {
            _groundType = groundType;
        }

        public void Cleanup()
        {
            TPController.Add -= TPAudio_Add;
            TPController.Remove -= TPAudio_Remove;
            TPController.Jumped -= TPAudio_Jumped;
            TPController.Landed -= TPAudio_Landed;
            TPAnimator.Footstep -= TPAudio_Footstep;
        }

        private void TPAudio_Remove()
        {
            _audioManager.Play(_removeData, _interactSource);
        }

        private void TPAudio_Add()
        {
            _audioManager.Play(_addData, _interactSource);
        }

        private void TPAudio_Footstep()
        {
            var array = _groundType is GroundType.Sand
                ? _sandFootstepsArray
                : _rockFootstepsArray;

            var index = Random.Range(0, array.Length);
            _audioManager.Play(array[index], _footstepSource);
        }
        private void TPAudio_Landed(Vector3 velocity)
        {
            if (velocity.magnitude > _minImpactMagnitude)
            {
                _jumpSource.Stop();
                _audioManager.Play(_landData, _landSource);
            }
        }

        private void TPAudio_Jumped()
        {
            _audioManager.Play(_jumpData, _jumpSource);
        }
    }
}