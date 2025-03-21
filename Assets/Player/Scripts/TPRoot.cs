namespace Context.ThirdPersonController
{
    using System;
    using UnityEngine;

    public class TPRoot : MonoBehaviour
    {
        private Transform _followTarget;
        private Transform _transform;

        public void Init(Transform followTarget)
        {
            _followTarget = followTarget;
            _transform = transform;
        }

        public void LateTick()
        {
            _transform.SetPositionAndRotation(_followTarget.position, _followTarget.rotation);  
        }
    }
}