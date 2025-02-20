namespace Context
{
    using UnityEngine;

    [RequireComponent(typeof(Collider))]
    public abstract class BaseConnectionPoint : MonoBehaviour
    {
        public Collider Collider => _collider;

        [field:SerializeField] public BaseConnectionPoint[] InitialConnections {  get; private set; }
        [field: SerializeField] public int MaxConnections { get; private set; } = 2;

        protected ConnectionManager _manager;
        protected Collider _collider;

        public virtual void Init(ConnectionManager connectionManager)
        {
            _manager = connectionManager;
            _collider = GetComponent<Collider>();
        }
    }
}