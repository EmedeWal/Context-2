namespace Context
{
    using System.Collections.Generic;
    using UnityEngine.Events;
    using UnityEngine;
    using System.Linq;

    [RequireComponent(typeof(Collider))]
    public abstract class BaseConnectionPoint : MonoBehaviour
    {
        public List<Connection> Connections;
        public Collider Collider { get; private set; }

        [field: SerializeField] public BaseConnectionPoint[] InitialConnectionPoints { get; private set; }

        [Header("Events")]
        [SerializeField] private UnityEvent _firstConnection;
        [SerializeField] private UnityEvent _allConnections;

        protected ConnectionManager _manager;

        protected const int MAX_CONNECTIONS = 2;

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

        public bool ConnectionsOverCap(int connectionIncrement) => Connections.Count + connectionIncrement > MAX_CONNECTIONS;

        public OtherConnectionStruct GetFirstOtherConnection()
        {
            var connection = Connections[0];
            var other = connection.AttachedPoints.FirstOrDefault(conn => conn != this);

            return new OtherConnectionStruct(other, connection);
        }

        public void ConnectionsStabilized()
        {
            var stableConnections = Connections.Where(c => c.Stable).ToList();

            if (stableConnections.Count == 1) OnFirstConnection();
            else if (stableConnections.Count == MAX_CONNECTIONS) OnAllConnections();
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
    }
}