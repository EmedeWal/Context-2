namespace Context
{
    using System.Collections.Generic;
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

            // First init
            var connectionPoints = FindObjectsByType<BaseConnectionPoint>(FindObjectsSortMode.None);
            foreach (var connectionPoint in connectionPoints)
                connectionPoint.Init(this);

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
            // Ensure dictionary contains the connection points
            if (!_connectionPoints.ContainsKey(connectionPointA))
                _connectionPoints.Add(connectionPointA, new List<Connection>());

            if (!_connectionPoints.ContainsKey(connectionPointB))
                _connectionPoints.Add(connectionPointB, new List<Connection>());

            // Return if either is at their connection cap
            if (_connectionPoints[connectionPointA].Count >= connectionPointA.MaxConnections || _connectionPoints[connectionPointB].Count >= connectionPointB.MaxConnections)
            {
                Debug.LogWarning("Requested connection is invalid! Either point has max connections!");
                return;
            }
            CreateConnection(connectionPointA, connectionPointB);
        }

        public void TransferConnection(BaseConnectionPoint connectionPointA, BaseConnectionPoint connectionPointB)
        {

        }

        private void CreateConnection(BaseConnectionPoint connectionPointA, BaseConnectionPoint connectionPointB)
        {
            // Instantiate and initialize the connection
            var connection = Instantiate(_connectionPrefab, transform);
            connection.Init(connectionPointA.Collider, connectionPointB.Collider);


            // Store the connection in both points
            _connectionPoints[connectionPointA].Add(connection);
            _connectionPoints[connectionPointB].Add(connection);
        }
    }
}