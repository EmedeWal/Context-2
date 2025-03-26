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

        [Header("REFERENCES")]

        [Space]
        [Header("Project")]
        [SerializeField] private Connection _connectionPrefab;

        [Space]
        [Header("Scene")]
        [SerializeField] private ParticleController[] _fireflyParticles;
        private List<StaticConnectionPoint> _staticConnectionPoints;
        private List<BaseConnectionPoint> _connectionPoints;

        [Range(0, 1)] public float TEST;

        private TerrainLayerColorChanger _terrainLayerColorChanger;
        private PostProcessingManager _postProcessingManager;

        private void Start()
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

            _staticConnectionPoints = _connectionPoints.OfType<StaticConnectionPoint>().ToList(); 

            _terrainLayerColorChanger = TerrainLayerColorChanger.Instance;
            _postProcessingManager = ApplicationManager.Instance.PostProcessingManager;

            foreach (var particle in _fireflyParticles)
                particle.Init();

            UpdatePostProcessing(1);

            for (int i = 0; i < _fireflyParticles.Length; i++)
                UpdateLevelBasedConnections(i);
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

            var tickedConnections = new HashSet<Connection>(); // Track already ticked connections
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
            UpdatePostProcessing(Time.deltaTime);

            for (int i = 0; i < _fireflyParticles.Length; i++)
                UpdateLevelBasedConnections(i);
        }

        private void UpdatePostProcessing(float deltaTime)
        {
            var finishedConnectionPoints = _connectionPoints.Where(point => point.HasMaxConnections()).ToList();
            var finishedPercentage = ((float)finishedConnectionPoints.Count / (float)_connectionPoints.Count);
            _postProcessingManager.UpdateVolumeSettings(Mathf.Clamp01(TEST), deltaTime);
        }

        private void UpdateLevelBasedConnections(int index)
        {
            var connectionPoints = _staticConnectionPoints.Where(point => point.LevelIndex == index).ToList();

            if (connectionPoints.Count == 0)
            {
                //Debug.LogWarning($"No connection points found for level {index}. Setting finishedPercentage to 0.");
                _terrainLayerColorChanger.ChangeLayerColor(index, 0f);
                _fireflyParticles[index].Tick(0f);
                return;
            }

            var finishedPoints = connectionPoints.Where(point => point.HasMaxConnections()).ToList();
            var finishedPercentage = (float)finishedPoints.Count / connectionPoints.Count;
            finishedPercentage = Mathf.Clamp01(finishedPercentage);

            _terrainLayerColorChanger.ChangeLayerColor(index, TEST);
            _fireflyParticles[index].Tick(TEST);
        }


        // Trigger enter
        public void CreateUnstableConnection(BaseConnectionPoint caller, BaseConnectionPoint target)
        {
            var data = caller.GetFirstOtherConnection();
            var callerTargetConnection = target.Connections.Any(c => c.AttachedPoints.Contains(caller));
            var targetOtherConnection = target.Connections.Any(c => c.AttachedPoints.Contains(data.Other));

            if ((targetOtherConnection && !callerTargetConnection) 
            || (target.ConnectionsOverCap(2) && !callerTargetConnection)
            || (target.Connections.Count == 1 && callerTargetConnection))
                return;

            if (!target.Connections.Contains(data.Connection))
            {
                TransferConnection(caller, data.Other, target, data.Connection, false);
                CreateConnection(caller, target, false);
            }

            // Ensure all connections are unobstructed before stabilizing
            if (data.Other.Connections.Any(connection => connection.Obstructed) || target.Connections.Any(connection => connection.Obstructed))
                return;

            WorldSpaceCanvas.Instance.ShowPrompt(target.transform.position, target.ControlPromptOffset, "Interact");
        }

        // Trigger exit
        public void RemoveUnstableConnection(BaseConnectionPoint caller, BaseConnectionPoint target)
        {
            WorldSpaceCanvas.Instance.HidePrompt();

            var callerData = caller.GetFirstOtherConnection();
            var callerTargetConnection = target.Connections.FirstOrDefault(c => c.AttachedPoints.Contains(caller));
            var targetOtherConnection = target.Connections.FirstOrDefault(c => c.AttachedPoints.Contains(callerData.Other));

            if (callerTargetConnection == null || targetOtherConnection == null)
                return;

            if (!callerTargetConnection.Stable && !targetOtherConnection.Stable)
            {
                var other = targetOtherConnection.AttachedPoints.FirstOrDefault(p => p != target);

                RemoveConnection(callerTargetConnection);
                TransferConnection(target, other, caller, targetOtherConnection, true);
            }
        }

        // Interacted with connection point. decide to add or undo
        public void InteractWithConnections(BaseConnectionPoint caller, BaseConnectionPoint target)
        {
            if (target.Connections.Any(c => !c.Stable)) 
                StabilizeConnections(target);
            else if (WorldSpaceCanvas.Instance.IsEnabled) 
                UnstabilizeConnections(caller, target);
        }

        // Press when connections are unstable
        private void StabilizeConnections(BaseConnectionPoint target)
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

            // trigger events
            target.ConnectionsModified();
        }

        // Press when connections are stable
        private void UnstabilizeConnections(BaseConnectionPoint caller, BaseConnectionPoint target)
        {
            var otherConnectionStruct = target.GetFirstOtherConnection();
            otherConnectionStruct.Connection.Stable = false;
            caller.Connections[0].Stable = false;

            target.ConnectionsModified();
        }

        private void RequestConnection(BaseConnectionPoint connectionPointA, BaseConnectionPoint connectionPointB)
        {
            // Return if either is at their c cap
            if (connectionPointA.ConnectionsOverCap(1) || connectionPointB.ConnectionsOverCap(1))
            {
                Debug.LogWarning("Requested connection is invalid! Either point has max connections!");
                return;
            }
            CreateConnection(connectionPointA, connectionPointB, true);
        }

        // Connection from a to b goes from b to c
        private void TransferConnection(BaseConnectionPoint connectionPointA, BaseConnectionPoint connectionPointB, BaseConnectionPoint connectionPointC, Connection connection, bool stable)
        {
            // Update the c to go from other to target instead
            connection.SetupConnection(connectionPointB, connectionPointC);

            // Update tracking dictionary
            connectionPointA.Connections.Remove(connection);
            connectionPointC.Connections.Add(connection);

            connection.Stable = stable;
        }

        private void CreateConnection(BaseConnectionPoint connectionPointA, BaseConnectionPoint connectionPointB, bool stable)
        {
            // Instantiate and initialize the connection. Do not parent due to weird local space mesh baking logic
            var connection = Instantiate(_connectionPrefab);
            connection.Init(connectionPointA, connectionPointB, stable);

            // Store the connection in both points
            connectionPointA.Connections.Add(connection);
            connectionPointB.Connections.Add(connection);

            connectionPointA.ConnectionsModified();
            connectionPointB.ConnectionsModified();
        }

        private void RemoveConnection(Connection connection)
        {
            // Collect the connection points that have this connection
            var connectedPoints = _connectionPoints.Where(point => point.Connections.Contains(connection)).ToList();

            // Remove the connection from each connection point
            foreach (var point in connectedPoints)
                point.Connections.Remove(connection);

            connection.Cleanup();
            Destroy(connection.gameObject);
        }

        public bool HasStableConnection(BaseConnectionPoint caller, BaseConnectionPoint target)
        {
            return caller.Connections.Any(connection =>
                connection.Stable && connection.AttachedPoints.Contains(target));
        }
    }
}