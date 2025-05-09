namespace Context
{
    using System.Collections.Generic;
    using UnityEngine.Events;
    using UnityEngine;
    using System.Linq;

    [RequireComponent(typeof(Collider))]
    public abstract class BaseConnectionPoint : MonoBehaviour
    {
        public List<Connection> Connections { get; private set; }  
        public Collider Collider { get; private set; }

        [field: SerializeField] public BaseConnectionPoint[] InitialConnectionPoints { get; private set; }
        [field: SerializeField] public float ControlPromptOffset { get; private set; }

        [Header("REFERENCES")]

        [Space]
        [Tooltip("Optional. Leave it empty if no dialogue is started when max connections are met.")]
        [SerializeField] private DialogueDataSO _dialogueData;

        [Space]
        [Tooltip("This event is invoked when max connections are met.")]
        [SerializeField] private UnityEvent _connectionAdded;
        [SerializeField] private UnityEvent _connectionRemoved;

        [Space]
        [SerializeField] protected int _maxconnections = 2;

        protected ConnectionManager _manager;
        protected bool _wasCompleted;

        public virtual void Init(ConnectionManager connectionManager)
        {
            Connections = new();
            Collider = GetComponent<Collider>();
            _wasCompleted = false;

            _manager = connectionManager;
        }

        public virtual void Cleanup()
        {
            foreach (var connection in Connections)
                connection.Cleanup();

            _connectionAdded = null;
            _connectionRemoved = null;
        }

        public virtual bool HasMaxConnections(Connection exception)
        {
            var stableConnections = exception != null
                ? Connections.Where(connection => connection.Stable || connection == exception).ToList()
                : Connections.Where(connection => connection.Stable).ToList();
            return stableConnections.Count >= _maxconnections;
        }
        public bool ConnectionsOverCap(int connectionIncrement) => Connections.Count + connectionIncrement > _maxconnections;

        public OtherConnectionStruct GetFirstOtherConnection()
        {
            var connection = Connections[0];
            var other = connection.AttachedPoints.FirstOrDefault(conn => conn != this);

            return new OtherConnectionStruct(other, connection);
        }

        public void ConnectionsModified()
        {
            var stableConnections = Connections.Where(c => c.Stable).ToList();
            if (stableConnections.Count == _maxconnections) OnCompletedConnections();
            else if (stableConnections.Count == 0 && _wasCompleted) OnIncompletedConnections();
        }

        protected virtual void OnCompletedConnections()
        {
            _wasCompleted = true;

            if (_dialogueData != null)
            {
                DialogueManager.Instance.StartDialogue(_dialogueData.Dialogue);
                _dialogueData = null;
            }
            _connectionAdded?.Invoke();
        }
        protected virtual void OnIncompletedConnections()
        {
            _connectionRemoved?.Invoke();
        }

    }
}