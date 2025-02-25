namespace Context
{
    using System.Linq;
    using UnityEngine;

    [RequireComponent(typeof(LineRenderer), typeof(MeshCollider))]
    public class Connection : MonoBehaviour
    {
        public MeshCollider MeshCollider => _meshCollider;
        public BaseConnectionPoint[] Connections { get; private set; }
        public bool Obstruced { get; private set; } 

        [Header("SETTINGS")]

        [Space]
        [Header("Colors")]
        [SerializeField] private Gradient _defaultGradient;
        [SerializeField] private Gradient _obstructedGradient;

        [Space]
        [Header("Size")]
        [SerializeField] private float _width = 0.15f;

        private LineRenderer _lineRenderer;
        private MeshCollider _meshCollider;
        private Mesh _bakedMesh;

        public void Init(BaseConnectionPoint pointA, BaseConnectionPoint pointB)
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _meshCollider = GetComponent<MeshCollider>();

            _lineRenderer.colorGradient = _defaultGradient;
            _lineRenderer.numCornerVertices = 6;
            _lineRenderer.numCapVertices = 6;
            _lineRenderer.positionCount = 2;
            _lineRenderer.startWidth = _width;
            _lineRenderer.endWidth = _width;

            SetupConnection(pointA, pointB);
        }

        public void Cleanup()
        {
            if (_meshCollider != null && _meshCollider.sharedMesh != null)
            {
                Destroy(_meshCollider.sharedMesh);
                _meshCollider.sharedMesh = null;
            }

            if (_bakedMesh != null)
            {
                Destroy(_bakedMesh);
                _bakedMesh = null;
            }
        }

        public void FixedTick()
        {
            var colliderA = Connections[0].Collider;
            var colliderB = Connections[1].Collider;
            var pointA = colliderA.bounds.center;
            var pointB = colliderB.bounds.center;

            Obstruced = IsObstructed(colliderA, colliderB, pointA, pointB);
            _lineRenderer.colorGradient = Obstruced
                ? _obstructedGradient
                : _defaultGradient;

            UpdateConnection(pointA, pointB);
        }

        public static bool IsObstructed(Collider colliderA, Collider colliderB, MeshCollider meshCollider, LayerMask layerMask)
        {
            var pointA = colliderA.bounds.center;
            var pointB = colliderB.bounds.center;

            // Offset the points slightly to avoid overlapping with colliders
            var start = colliderA.ClosestPoint(pointB);
            var end = colliderB.ClosestPoint(pointA);

            var dis = Vector3.Distance(start, end);
            var dir = (pointB - pointA).normalized; // Normalize direction to avoid issues
            var radius = 0.01f;

            var hits = Physics.SphereCastAll(start, radius, dir, dis, layerMask);
            return hits.Any(hit =>
                hit.collider != null &&
                hit.collider != colliderA && // Exclude its own connection point A
                hit.collider != colliderB && // Exclude its own connection point B
                hit.collider != meshCollider 
            );
        }

        public void SetupConnection(BaseConnectionPoint a, BaseConnectionPoint b)
        {
            Connections = new BaseConnectionPoint[2] { a, b };
            UpdateConnection(Connections[0].Collider.bounds.center, Connections[1].Collider.bounds.center);
        }

        public void UpdateConnection(Vector3 pointA, Vector3 pointB)
        {
            UpdateLinePoints(pointA, pointB);
            UpdateMeshCollider();
        }

        private bool IsObstructed(Collider colliderA, Collider colliderB, Vector3 pointA, Vector3 pointB)
        {
            // Offset the points slightly to avoid overlapping with colliders
            var start = colliderA.ClosestPoint(pointB);
            var end = colliderB.ClosestPoint(pointA);

            var dis = Vector3.Distance(start, end);
            var dir = (pointB - pointA).normalized; // Normalize direction to avoid issues
            var radius = 0.01f;

            var hits = Physics.SphereCastAll(start, radius, dir, dis);
            return hits.Any(hit =>
                hit.collider != null &&
                hit.collider != _meshCollider &&  // Exclude its own mesh collider
                hit.collider != colliderA &&     // Exclude its own connection point A
                hit.collider != colliderB        // Exclude its own connection point B
            );
        }

        private void UpdateLinePoints(Vector3 pointA, Vector3 pointB)
        {
            _lineRenderer.SetPosition(0, pointA);
            _lineRenderer.SetPosition(1, pointB);
        }

        public void UpdateMeshCollider()
        {
            if (_bakedMesh != null) Destroy(_bakedMesh); // Destroy previous mesh to free memory

            _bakedMesh = new Mesh();
            _lineRenderer.BakeMesh(_bakedMesh, true);
            _meshCollider.sharedMesh = _bakedMesh;

            _meshCollider.convex = true;
            _meshCollider.isTrigger = true;
        }
    }
}