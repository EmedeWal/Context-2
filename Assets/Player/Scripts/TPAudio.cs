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
        [SerializeField] private AudioData[] _footstepArray;
        [SerializeField] private AudioData _removeData;
        [SerializeField] private AudioData _addData;
        [SerializeField] private AudioData _jumpData;
        [SerializeField] private AudioData _landData;

        private AudioManager _audioManager;

        public void Init()
        {
            _audioManager = ApplicationManager.Instance.AudioManager;

            TPController.Add += TPAudio_Add;
            TPController.Remove += TPAudio_Remove;
            TPController.Jumped += TPAudio_Jumped;
            TPController.Landed += TPAudio_Landed;
            TPAnimator.Footstep += TPAudio_Footstep;
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
            var index = Random.Range(0, _footstepArray.Length);
            _audioManager.Play(_footstepArray[index], _footstepSource);
        }
        private void TPAudio_Landed()
        {
            _jumpSource.Stop();
            _audioManager.Play(_landData, _landSource);
        }

        private void TPAudio_Jumped()
        {
            _audioManager.Play(_jumpData, _jumpSource);
        }
    }
}