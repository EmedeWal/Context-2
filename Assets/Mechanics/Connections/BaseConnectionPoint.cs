namespace Context
{
    using System.Collections.Generic;
    using UnityEngine.Events;
    using UnityEngine;
    using System.Linq;

    [RequireComponent(typeof(Collider))]
    public abstract class BaseConnectionPoint : MonoBehaviour
    {
        public List<Connection> Connections; // private setter later
        public Collider Collider { get; private set; }

        [Header("REFERENCES")]

        [Header("Connections")]
        [field: SerializeField] public BaseConnectionPoint[] InitialConnectionPoints { get; private set; }
        [SerializeField] protected int _maxConnections;

        [Space]
        [Header("Events")]
        [SerializeField] private UnityEvent _firstConnection;
        [SerializeField] private UnityEvent _allConnections;

        protected ConnectionManager _manager;

        public virtual void Init(ConnectionManager connectionManager)
        {
            Connections = new();
            Collider = GetComponent<Collider>();

            _manager = connectionManager;
        }

        public virtual void Cleanup()
        {
            foreach (var connection in Connections)
                connection.Cleanup();

            _firstConnection = null;
            _allConnections = null;
        }

        public bool HasMaxConnections() => Connections.Count >= _maxConnections; 

        public void ConnectionsStabilized()
        {
            var stableConnections = Connections.Where(c => c.Stable).ToList();

            if (stableConnections.Count == 1) OnFirstConnection();
            else if (stableConnections.Count == _maxConnections) OnAllConnections();
        }

        protected virtual void OnFirstConnection()
        {
            _firstConnection?.Invoke();
            _firstConnection = null;
        }

        protected virtual void OnAllConnections()
        {
            _allConnections?.Invoke();
            _allConnections = null;
        }

        protected  OtherConnectionStruct GetFirstOtherConnection()
        {
            var connection = Connections[0];
            var other = connection.AttachedPoints.FirstOrDefault(conn => conn != this);

            return new OtherConnectionStruct(other, connection);
        }
    }
}