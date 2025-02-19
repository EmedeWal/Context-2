namespace Context
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using System;

    public class ConnectionManager : MonoBehaviour
    {
        public static ConnectionManager Instance;

        [SerializeField] private Connection _connectionPrefab;

        private List<Connection> _connections;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else DestroyImmediate(gameObject);

            _connections = new();
        }

        private void LateUpdate()
        {
            foreach (var connection in _connections)
                connection.LateTick();
        }

        public void TransferConnection(Collider player, Collider target)
        {
            if (HasExistingConnection(player, target))
                return; 

            var connections = new List<Connection>(_connections);
            foreach (var connection in connections)
            {
                if (connection.Colliders.Contains(player))
                {
                    var index = Array.IndexOf(connection.Colliders, player);
                    var other = connection.Colliders[(index + 1) % connection.Colliders.Length];

                    if (HasExistingConnection(target, other))
                        return;

                    connection.UpdateConnection(target, index);
                    CreateConnection(target, player);
                }
            }
        }

        public void CreateConnection(Collider a, Collider b)
        {
            if (HasExistingConnection(a, b))
                return;

            var connection = Instantiate(_connectionPrefab, transform);
            connection.Init(a, b);

            _connections.Add(connection);
        }

        private bool HasExistingConnection(Collider a, Collider b) => _connections.Any(c => c.Colliders.Contains(a) && c.Colliders.Contains(b));
    }
}
