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


        public void Init()
        {

        }

        public void Cleanup()
        {

        }
    }
}