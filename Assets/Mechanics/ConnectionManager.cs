namespace Context
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class ConnectionManager : MonoBehaviour
    {
        public static ConnectionManager Instance;

        [SerializeField] private Connection _connectionPrefab;

        private Dictionary<BaseConnectionPoint, List<Connection>> _connectionPoints; // Track connections per collider

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
                _connectionPoints.Add(connectionPoint, new List<Connection>());
                connectionPoint.Init(this);
            }

            // then make initial connections
            foreach (var connectionPoint in connectionPoints)
                foreach (var other in connectionPoint.InitialConnections)
                    RequestConnection(connectionPoint, other);
        }

        private void FixedUpdate()
        {
            HashSet<Connection> tickedConnections = new(); // Track already ticked connections
            foreach (var connectionList in _connectionPoints.Values)
            {
                foreach (var connection in connectionList)
                {
                    if (!tickedConnections.Contains(connection))
                    {
                        connection.FixedTick();
                        tickedConnections.Add(connection);
                    }
                }
            }
        }

        public void RequestConnection(BaseConnectionPoint connectionPointA, BaseConnectionPoint connectionPointB)
        {
            // Return if either is at their c cap
            if (HasMaxContains(connectionPointA) || HasMaxContains(connectionPointB))
            {
                Debug.LogWarning("Requested connection is invalid! Either point has max connections!");
                return;
            }
            CreateConnection(connectionPointA, connectionPointB);
        }

        public void InteractWithConnection(BaseConnectionPoint caller, BaseConnectionPoint target)
        {
            var callerTargetConnection = _connectionPoints[caller][0]; // Get the caller connection to the target
            var other = callerTargetConnection.Connections.FirstOrDefault(conn => conn != caller); // Get the other connection attached to the caller. Can only be one so first or default is good

            if (BlockConnectionRequest(target, other, callerTargetConnection, out var warning))
            {
                Debug.LogWarning(warning);
                return;
            }
            TransferConnection(caller, other, target, callerTargetConnection);
            CreateConnection(caller, target);
        }

        // Connection from a to b goes from b to c
        private void TransferConnection(BaseConnectionPoint connectionPointA, BaseConnectionPoint connectionPointB, BaseConnectionPoint connectionPointC, Connection connection)
        {
            // Update the c to go from other to target instead
            connection.SetupConnection(connectionPointB, connectionPointC);

            // Update tracking dictionary
            _connectionPoints[connectionPointA].Remove(connection);
            _connectionPoints[connectionPointC].Add(connection);
        }

        private void CreateConnection(BaseConnectionPoint connectionPointA, BaseConnectionPoint connectionPointB)
        {
            // Instantiate and initialize the c
            var connection = Instantiate(_connectionPrefab, transform);
            connection.Init(connectionPointA, connectionPointB);

            // Store the c in both points
            _connectionPoints[connectionPointA].Add(connection);
            _connectionPoints[connectionPointB].Add(connection);
        }

        private void RemoveConnection(Connection connection)
        {
            var pointsToUpdate = new List<BaseConnectionPoint>();   

            // Collect keys where the connection exists
            foreach (var kvp in _connectionPoints)
                if (kvp.Value.Contains(connection))
                    pointsToUpdate.Add(kvp.Key);

            // Remove the connection from the collected keys
            foreach (var point in pointsToUpdate)
                _connectionPoints[point].Remove(connection);

            connection.Cleanup();
            Destroy(connection.gameObject);
        }

        private bool HasMaxContains(BaseConnectionPoint point) => _connectionPoints[point].Count >= point.MaxConnections;
        private bool BlockConnectionRequest(BaseConnectionPoint target, BaseConnectionPoint other, Connection callerTargetConnection, out string warning)
        {
            if (_connectionPoints[target].Contains(callerTargetConnection))
            {
                warning = "Caller is already connected to the target!";
                return true;
            }

            var obstructed = Connection.IsObstructed(target.Collider, other.Collider, callerTargetConnection.MeshCollider, _layerMask);
            if (callerTargetConnection.Obstruced || obstructed)
            {
                warning = "Connection to caller is obstructed, cannot connect!";
                return true;
            }
            if (HasMaxContains(target))
            {
                warning = "Target has max connections!";
                return true;
            }
            if (_connectionPoints[target].Any(c => c.Connections.Contains(other)))
            {
                warning = "Target was already connected with the other.";
                return true;
            }
            warning = string.Empty;
            return false;
        }
    }
}