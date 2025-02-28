namespace Context
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public struct OtherConnectionStruct
    {
        public BaseConnectionPoint Other;
        public Connection Connection;

        public OtherConnectionStruct(BaseConnectionPoint other, Connection connection)
        {
            Other = other;  
            Connection = connection;
        }
    }

    public class ConnectionManager : MonoBehaviour
    {
        public static ConnectionManager Instance { get; private set; }

        [SerializeField] private Connection _connectionPrefab;

        private List<BaseConnectionPoint> _connectionPoints;

        [SerializeField] private LayerMask _layerMask;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else DestroyImmediate(gameObject);

            _connectionPoints = new();

            // First init and collect
            var connectionPoints = FindObjectsByType<BaseConnectionPoint>(FindObjectsSortMode.None);
            foreach (var connectionPoint in connectionPoints)
            {
                _connectionPoints.Add(connectionPoint);
                connectionPoint.Init(this);
            }

            // then make initial connections
            foreach (var connectionPoint in connectionPoints)
                foreach (var other in connectionPoint.InitialConnectionPoints)
                    RequestConnection(connectionPoint, other);
        }

        private void OnDisable()
        {
            foreach (var connectionPoint in _connectionPoints)
                connectionPoint.Cleanup();
        }

        private void LateUpdate()
        {
            var player = _connectionPoints.FirstOrDefault(point => point.transform.CompareTag("Player"));
            var playerConnectionCollider = player.Connections[0].MeshCollider;

            HashSet<Connection> tickedConnections = new(); // Track already ticked connections
            foreach (var point in _connectionPoints)
            {
                foreach (var connection in point.Connections)
                {
                    if (!tickedConnections.Contains(connection))
                    {
                        connection.LateTick(playerConnectionCollider);
                        tickedConnections.Add(connection);
                    }
                }
            }
        }

        public void RequestConnection(BaseConnectionPoint connectionPointA, BaseConnectionPoint connectionPointB)
        {
            // Return if either is at their c cap
            if (connectionPointA.HasMaxConnections() || connectionPointB.HasMaxConnections())
            {
                Debug.LogWarning("Requested connection is invalid! Either point has max connections!");
                return;
            }
            CreateConnection(connectionPointA, connectionPointB, true);
        }

        public void CreateUnstableConnection(BaseConnectionPoint caller, BaseConnectionPoint target, OtherConnectionStruct data)
        {
            if (BlockConnectionRequest(target, data.Other, data.Connection, out var warning))
            {
                Debug.LogWarning(warning);
                return;
            }
            TransferConnection(caller, data.Other, target, data.Connection, false);
            CreateConnection(caller, target, false);
        }

        public void RemoveUnstableConnection(BaseConnectionPoint caller, BaseConnectionPoint target, OtherConnectionStruct oldData, OtherConnectionStruct newData)
        {
            if (!oldData.Connection.Stable && !newData.Connection.Stable)
            {
                RemoveConnection(newData.Connection);
                TransferConnection(target, oldData.Other, caller, oldData.Connection, true);
            }
        }

        public void StabilizeConnections(BaseConnectionPoint target)
        {
            // Ensure all connections are unobstructed before stabilizing
            if (target.Connections.Any(connection => connection.Obstructed))
            {
                Debug.LogWarning("Cannot stabilize connections: One or more connections are obstructed!");
                return;
            }

            // If all are unobstructed, mark them as stable
            foreach (var connection in target.Connections)
                connection.Stable = true;
        }

        // Connection from a to b goes from b to c
        private void TransferConnection(BaseConnectionPoint connectionPointA, BaseConnectionPoint connectionPointB, BaseConnectionPoint connectionPointC, Connection connection, bool stable)
        {
            // Update the c to go from other to target instead
            connection.SetupConnection(connectionPointB, connectionPointC);

            // Update tracking dictionary
            connectionPointA.RemoveConnection(connection);
            connectionPointC.AddConnection(connection);

            connection.Stable = stable;
        }

        private void CreateConnection(BaseConnectionPoint connectionPointA, BaseConnectionPoint connectionPointB, bool stable)
        {
            // Instantiate and initialize the connection
            var connection = Instantiate(_connectionPrefab, transform);
            connection.Init(connectionPointA, connectionPointB, stable);

            // Store the connection in both points
            connectionPointA.AddConnection(connection);
            connectionPointB.AddConnection(connection);
        }

        private void RemoveConnection(Connection connection)
        {
            // Collect the connection points that have this connection
            var connectedPoints = _connectionPoints.Where(point => point.Connections.Contains(connection)).ToList();

            // Remove the connection from each connection point
            foreach (var point in connectedPoints)
                point.RemoveConnection(connection);

            connection.Cleanup();
            Destroy(connection.gameObject);
        }

        //private bool HasMaxContains(BaseConnectionPoint point) => _connectionPoints[point].Count >= point._maxConnections;
        private bool BlockConnectionRequest(BaseConnectionPoint target, BaseConnectionPoint other, Connection callerTargetConnection, out string warning)
        {
            if (target.Connections.Contains(callerTargetConnection))
            {
                warning = "Caller is already connected to the target!";
                return true;
            }
            if (target.HasMaxConnections())
            {
                warning = "Target has max connections!";
                return true;
            }
            if (target.Connections.Any(c => c.AttachedPoints.Contains(other)))
            {
                warning = "Target was already connected with the other.";
                return true;
            }
            warning = string.Empty;
            return false;
        }
    }
}