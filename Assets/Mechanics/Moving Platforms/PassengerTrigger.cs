namespace Context
{
    using Context.ThirdPersonController;
    using UnityEngine;

    [RequireComponent(typeof(Collider))]
    public class PassengerTrigger : MonoBehaviour
    {
        public TPController Passenger { get; private set; }

        private MovingPlatform _movingPlatform;

        public void Init(MovingPlatform movingPlatform)
        {
            _movingPlatform = movingPlatform;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.TryGetComponent(out TPController controller))
            {
                if (_movingPlatform.ActivationMode is ActivationMode.Collision)
                    _movingPlatform.Activate();

                Passenger = controller;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.transform.TryGetComponent(out TPController controller))
            {
                Passenger = null;
            }
        }
    }
}