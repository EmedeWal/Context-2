namespace Context.ThirdPersonController
{
    using UnityEngine;

    public class TPAudio : MonoBehaviour
    {
        [SerializeField] private AudioSource _landSource;
        [SerializeField] private AudioSource _jumpSource;
        [SerializeField] private AudioSource _interactSource;
        [SerializeField] private AudioSource _footstepSource;

        [SerializeField] private AudioData[] _footstepArray;
        [SerializeField] private AudioData _interactData;
        [SerializeField] private AudioData _jumpData;
        [SerializeField] private AudioData _landData;

        private AudioManager _audioManager;

        public void Init()
        {
            _audioManager = ApplicationManager.Instance.AudioManager;

            TPController.InteractionCanceled += TPAudio_InteractionCanceled;
            TPController.InteractionStarted += TPAudio_InteractionStarted;
            TPController.Jumped += TPAudio_Jumped;
            TPController.Landed += TPAudio_Landed;
            TPRoot.Footstep += TPAudio_Footstep;
        }

        public void Cleanup()
        {
            TPController.InteractionCanceled -= TPAudio_InteractionCanceled;
            TPController.InteractionStarted -= TPAudio_InteractionStarted;
            TPController.Jumped -= TPAudio_Jumped;
            TPController.Landed -= TPAudio_Landed;
            TPRoot.Footstep -= TPAudio_Footstep;
        }

        private void TPAudio_InteractionStarted() { }
        private void TPAudio_InteractionCanceled() => _interactSource.Stop();

        private void TPAudio_Footstep() { }

        private void TPAudio_Landed() { }
        private void TPAudio_Jumped() { }
    }
}