namespace Context
{
    using System.Collections.Generic;
    using System.Linq;
    using Unity.VisualScripting;
    using UnityEngine;

    public class ConnectionManager : MonoBehaviour
    {
        public static ConnectionManager Instance;

        [SerializeField] private Connection _connectionPrefab;

        private Dictionary<BaseConnectionPoint, List<Connection>> _connectionPoints; // Track connections per collider

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
            if (HasAnyConnections(connectionPointA) || HasAnyConnections(connectionPointB))
            {
                Debug.LogWarning("Requested connection is invalid! Either point has max connections!");
                return;
            }
            CreateConnection(connectionPointA, connectionPointB);
        }

        public void InteractWithConnection(BaseConnectionPoint player, BaseConnectionPoint target)
        {
            // Get the player playerTargetConnection
            var playerTargetConnection = _connectionPoints[player][0];

            if (_connectionPoints[target].Contains(playerTargetConnection))
            {
                // Find the connection of the target that is NOT linked to the player
                var targetOtherConnection = _connectionPoints[target]
                    .FirstOrDefault(conn => !conn.Connections.Contains(player));

                if (targetOtherConnection == null)
                {
                    Debug.LogWarning("target had no other connection!");
                    return;
                }

                var other = targetOtherConnection.Connections.FirstOrDefault(conn => conn != target);
                TransferConnection(target, player, other, playerTargetConnection);

                RemoveConnection(targetOtherConnection);
            }
            else
            {
                if (playerTargetConnection.Obstruced)
                {
                    Debug.LogWarning("Connection to player is obstructed, cannot connect!");
                    return;
                }

                var other = playerTargetConnection.Connections.FirstOrDefault(conn => conn != player);

                // If the target already has max connections OR is already connected to other, return
                if (HasAnyConnections(target) || _connectionPoints[target].Any(c => c.Connections.Contains(other)))
                {
                    Debug.LogWarning("Target was already connected with the other.");
                    return;
                }

                TransferConnection(player, other, target, playerTargetConnection);
                CreateConnection(player, target);
            }
        }

        // Connection from a to b goes from b to c
        private void TransferConnection(BaseConnectionPoint connectionPointA, BaseConnectionPoint connectionPointB, BaseConnectionPoint connectionPointC, Connection connection)
        {
            // Update the c to go from other to target instead
            connection.SetupConnection(connectionPointB, connectionPointC);

            // Update tracking dictionary
            _connectionPoints[connectionPointA].Remove(connection);
            //_connectionPoints[connectionPointB].Remove(connection);

            //_connectionPoints[connectionPointB].Add(connection);
            _connectionPoints[connectionPointC].Add(connection);

            connectionPointA.CurrentConnections--;
            //connectionPointB.CurrentConnections--;

            //connectionPointB.CurrentConnections++;
            connectionPointC.CurrentConnections++;
        }

        private void CreateConnection(BaseConnectionPoint connectionPointA, BaseConnectionPoint connectionPointB)
        {
            // Instantiate and initialize the c
            var connection = Instantiate(_connectionPrefab, transform);
            connection.Init(connectionPointA, connectionPointB);

            // Store the c in both points
            _connectionPoints[connectionPointA].Add(connection);
            _connectionPoints[connectionPointB].Add(connection);

            connectionPointA.CurrentConnections++;
            connectionPointB.CurrentConnections++;
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
            {
                _connectionPoints[point].Remove(connection);
                point.CurrentConnections--;
            }

            connection.Cleanup();
            Destroy(connection.gameObject);
        }

        private bool HasAnyConnections(BaseConnectionPoint point) => _connectionPoints[point].Count > 0;
    }
}