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
            if (HasMaxConnections(connectionPointA) || HasMaxConnections(connectionPointB))
            {
                Debug.LogWarning("Requested connection is invalid! Either point has max connections!");
                return;
            }
            CreateConnection(connectionPointA, connectionPointB);
        }

        public void InteractWithConnection(BaseConnectionPoint player, BaseConnectionPoint target)
        {
            // Find the other c point attached to the player
            var connection = _connectionPoints[player][0];
            var other = connection.Connections.FirstOrDefault(conn => conn != player);

            if (_connectionPoints[target].Contains(connection))
            {
                Debug.Log("thou wists tho remove thine connection.");
            }
            else
            {
                // If the target already has max connections OR is already connected to other, return
                if (HasMaxConnections(target) || _connectionPoints[target].Any(c => c.Connections.Contains(other)))
                {
                    Debug.LogWarning("Target was already connected with the other or has max connections.");
                    return;
                }

                TransferConnection(other, target, player, connection);
                CreateConnection(player, target);
            }
        }

        private void TransferConnection(BaseConnectionPoint connectionPointA, BaseConnectionPoint connectionPointB, BaseConnectionPoint connectionPointC, Connection connection)
        {
            // Update the c to go from other to target instead
            connection.SetupConnection(connectionPointA, connectionPointB);

            // Update tracking dictionary
            _connectionPoints[connectionPointC].Remove(connection);
            _connectionPoints[connectionPointB].Add(connection);
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

        private bool HasMaxConnections(BaseConnectionPoint point) => _connectionPoints[point].Count >= point.MaxConnections;
    }
}