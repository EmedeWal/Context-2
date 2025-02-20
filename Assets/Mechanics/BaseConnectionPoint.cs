namespace Context
{
    using UnityEngine;

    [RequireComponent(typeof(Collider))]
    public abstract class BaseConnectionPoint : MonoBehaviour
    {
        public BaseConnectionPoint[] InitialConnections => _initialConnections;
        public Collider Collider => _collider;
        public int MaxConnections => _maxConnections;

        public int CurrentConnections;


        [SerializeField] private BaseConnectionPoint[] _initialConnections;
        [SerializeField, Range(1, 2)] private int _maxConnections = 2;

        protected ConnectionManager _manager;
        protected Collider _collider;

        public virtual void Init(ConnectionManager connectionManager)
        {
            _manager = connectionManager;
            _collider = GetComponent<Collider>();
        }
    }
}