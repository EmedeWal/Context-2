namespace Context
{
    using UnityEngine;

    [RequireComponent(typeof(Animator))]
    public class WorldSpaceUI : MonoBehaviour
    {
        private Transform _mainCamera;
        private Transform _transform;

        private void Start()
        {
            _mainCamera = Camera.main.transform;
            _transform = transform;
        }

        private void LateUpdate()
        {
            // Face the camera
            _transform.LookAt(transform.position + _mainCamera.forward);
        }
    }
}