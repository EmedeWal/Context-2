namespace Context
{
    using System;
    using UnityEngine;

    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class TriggerChannel : MonoBehaviour
    {
        private BaseConnectionPoint _point;

        public event Action<BaseConnectionPoint> ConnectionExit;
        public event Action<BaseConnectionPoint> ConnectionEnter;

        public void Init(BaseConnectionPoint point, float range)
        {
            _point = point;

            var collider = GetComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = range;

            var rigidbody = GetComponent<Rigidbody>();
            rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            rigidbody.isKinematic = true;

            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        }

        public void Cleanup()
        {
            ConnectionEnter = null;
            ConnectionExit = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out BaseConnectionPoint point) && point != _point)
                ConnectionEnter.Invoke(point);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out BaseConnectionPoint point) && point != _point)
                ConnectionExit.Invoke(point);
        }
    }
}