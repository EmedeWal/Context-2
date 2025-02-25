namespace Context
{
    using UnityEngine.Events;
    using UnityEngine;

    [RequireComponent(typeof(Collider))]
    public abstract class BaseConnectionPoint : MonoBehaviour
    {
        [field: SerializeField] public BaseConnectionPoint[] InitialConnections { get; private set; }
        [field: SerializeField] public int MaxConnections { get; private set; } = 2;

        public Collider Collider { get; protected set; }

        protected ConnectionManager _manager;

        [Header("EVENTS")]
        [SerializeField] private UnityEvent _firstConnection;
        [SerializeField] private UnityEvent _allConnections;

        public virtual void Init(ConnectionManager connectionManager)
        {
            _manager = connectionManager;
            Collider = GetComponent<Collider>();
        }

        public virtual void Cleanup()
        {
            _firstConnection = null;
            _allConnections = null;
        }

        public void OnConnectionModified(int connections)
        {
            if (connections == 1)
                OnFirstConnection();
            else if (connections == MaxConnections)
                OnAllConnections();
        }

        protected virtual void OnFirstConnection()
        {
            _firstConnection?.Invoke();
            _firstConnection = null;

            Debug.Log("First Connection event invoked!");
        }

        protected virtual void OnAllConnections()
        {
            _allConnections?.Invoke();
            _allConnections = null;

            Debug.Log("All Connections event invoked!");
        }
    }
}